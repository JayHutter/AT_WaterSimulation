using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidSim
{
    int size;
    float diffusion;
    float viscosity;

    float[,] temp;
    float[,] density;

    float[,] velocityX;
    float[,] velocityY;

    float[,] velocityX0;
    float[,] velocityY0;

    int accuracy;

    enum DataType
    {
        OTHER = 0,
        XVELOCITY,
        YVELOCITY
    }


    public FluidSim(int size, float diffusion, float viscosity, int accuracy)
    {
        this.size = size;
        this.diffusion = diffusion;
        this.viscosity = viscosity;

        temp = new float[size, size];
        density = new float[size, size];

        velocityX = new float[size, size];
        velocityY = new float[size, size];
        velocityX0 = new float[size, size];
        velocityY0 = new float[size, size];

        this.accuracy = accuracy;
    }

    // At the edge of the grid reflect the water inwards
    void SetBounds(int type, float[,] data)
    {
        /*
         * Far Left and Far Right cells
         * If input y velocity - boundry cell y vel is opposite neighbour
         * If input isnt y vel - boundry cell y vel is the same as neighbour
        */
        for (int x = 1; x < size - 1; x++)
        {
            data[x, 0] = type == 3 ?
                -data[x, 1] : data[x, 1];

            data[x, size - 1] = type == 3 ?
                -data[x, size - 2] : data[x, size - 2];
        }

        /* 
         * Top and Bottom cells
         * If input x velocity - boundry cell x vel is opposite neighbour
         * If input isnt x vel- boundry cell x vel is the same as neighbour
        */
        for (int y = 1; y < size - 1; y++)
        {
            data[0, y] = type == 1 ?
                -data[1, y] : data[1, y];

            data[size - 1, y] = type == 1 ?
                -data[size - 2, y] : data[size - 2, y];
        }

        //Set corner cell value to the average of its neighbours
        float reflectScale = 0.5f;
        data[0, 0] = reflectScale * data[1, 0] + data[0, 1];
        data[0, size - 1] = reflectScale * data[1, size - 1] + data[0, size - 2];
        data[size - 1, 0] = reflectScale * data[size - 2, 0] + data[size - 1, 1];
        data[size - 1, size - 1] = reflectScale * data[size - 2, size - 1] + data[size - 1, size - 2];
    }

    //Combines the data of neighbouring cells
    void LinearSolve(DataType type, float[,] data, float[,] data0, float a, float c)
    {
        float reciprocal = 1.0f / c;
        for (int i = 0; i < accuracy; i++)
        {
            for (int x = 1; x < size - 1; x++)
            {
                for (int y = 1; y < size - 1; y++)
                {
                    data[x, y] = (data0[x, y] + a
                        * (data[x + 1, y] + data[x - 1, y]
                        + data[x, y + 1] + data[x, y - 1]))
                        * reciprocal;
                }
            }

            //Update boundry data
            SetBounds((int)type, data);
        }
    }

    void Diffuse(DataType type, float[,] data, float[,] data0, float d)
    {
        float a = Time.deltaTime * d * (size - 2) * (size - 2);
        LinearSolve(type, data, data0, a, 1 + 6 * a);
    }

    /* 
     * Update the velocities in each cell
     * Simulate the movement over time to find end cell
     * Calculate weighted average of neighbour cells
     * Applies average to the cell
     */
    void Advect(DataType type, float[,] data, float[,] data0, float[,] velX, float[,] velY)
    {
        float dtX = Time.deltaTime * (size - 2);
        float dtY = Time.deltaTime * (size - 2);

        for (int x = 1; x < size - 1; x++)
        {
            for (int y = 1; y < size - 1; y++)
            {
                float tempX = velX[x, y] * dtX;
                float tempY = velY[x, y] * dtY;

                float xVel = x - tempX;
                float yVel = y - tempY;

                if (xVel < 0.5f) xVel = 0.5f;
                if (xVel > size + 0.5f) xVel = size + 0.5f;
                int x0 = Mathf.FloorToInt(xVel);
                int x1 = x0 + 1;

                if (yVel < 0.5f) yVel = 0.5f;
                if (yVel > size + 0.5f) yVel = size + 0.5f;
                int y0 = Mathf.FloorToInt(yVel);
                int y1 = y0 + 1;

                float s1 = xVel - x0;
                float s0 = 1.0f - s1;
                float t1 = yVel - y0;
                float t0 = 1.0f - t1;

                if (x0 >= size || x1 >= size ||
                    y0 >= size || y1 >= size ||
                    x >= size || y >= size ||
                    x0 <= 0 || x1 <= 0 ||
                    y0 <= 0 || y1 <= 0 ||
                    x <= 0 || y <= 0)
                {
                    continue;
                }

                data[x, y] =
                    s0 * (t0 * data0[x0, y0] + t1 * data0[x0, y1]) +
                    s1 * (t0 * data0[x1, y0] + t1 * data0[x1, y1]);
            }
        }

        SetBounds((int)type, data);
    }

    void Project(float[,] velX, float[,] velY, float[,] p, float[,] div)
    {
        for (int x = 1; x < size - 1; x++)
        {
            for (int y = 1; y < size - 1; y++)
            {
                div[x, y] = -0.5f * (
                    velX[x + 1, y] - velX[x - 1, y] +
                    velY[x, y + 1] - velY[x, y - 1]) / size;

                p[x, y] = 0;
            }
        }

        SetBounds((int)DataType.OTHER, div);
        SetBounds((int)DataType.OTHER, p);
        LinearSolve((int)DataType.OTHER, p, div, 1, 6);

        for (int x = 1; x < size - 1; x++)
        {
            for (int y = 1; y < size - 1; y++)
            {
                velX[x, y] -= 0.5f * (p[x + 1, y] - p[x - 1, y]) * size;
                velY[x, y] -= 0.5f * (p[x, y + 1] - p[x, y - 1]) * size;
            }
        }

        SetBounds((int)DataType.XVELOCITY, velX);
        SetBounds((int)DataType.YVELOCITY, velY);
    }

    public void Update()
    {
        Diffuse(DataType.XVELOCITY, velocityX0, velocityX, viscosity);
        Diffuse(DataType.YVELOCITY, velocityY0, velocityY, viscosity);

        Project(velocityX0, velocityY0, velocityX, velocityY);

        Advect(DataType.XVELOCITY, velocityX, velocityX0, velocityX0, velocityY0);
        Advect(DataType.YVELOCITY, velocityY, velocityY0, velocityX0, velocityY0);

        Project(velocityX, velocityY, velocityX0, velocityY0);

        Diffuse(DataType.OTHER, temp, density, diffusion);
        Advect(DataType.OTHER, density, temp, velocityX, velocityY);
    }

    public void ApplyForceAt(int x, int y, Vector2 velocity, float strength)
    {
        ApplyForceAt(x, y, velocity.x, velocity.y, strength);
    }

    public void ApplyForceAt(int x, int y, float xVel, float yVel, float strength)
    {
        if (x < 0 || y < 0 || 
            x >= size || y >= size)
        {
            return;
        }

        velocityX[x, y] += xVel;
        velocityY[x, y] += yVel;
        density[x, y] += strength;
    }

    public void OutputDenisities()
    {
        string text = "Densities: \n";
        for (int x=0; x< size-1; x++)
        {
            for (int y=0; y < size-1; y++)
            {
                text += density[x, y];

                text += ", ";
            }
            text += "\n";
        }

        Debug.Log(text);
    }

    public int Size()
    {
        return size;
    }

    public float VelocityX(int x, int y)
    {
        return velocityX[x, y];
    }

    public float VelocityY(int x, int y)
    {
        return velocityY[x, y];
    }

    public float Density(int x, int y)
    {
        return density[x, y];
    }
}
