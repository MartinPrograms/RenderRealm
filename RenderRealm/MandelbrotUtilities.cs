using System.Numerics;

namespace RenderRealm;

public static class MandelbrotUtilities
{
    public static int Mandelbrot(float x, float y, int maxIterations)
    {
        float a = 0;
        float b = 0;
        int n = 0;
        while (n < maxIterations)
        {
            float aa = a * a;
            float bb = b * b;
            if (aa + bb > 4.0f)
            {
                break;
            }
            float twoab = 2.0f * a * b;
            a = aa - bb + x;
            b = twoab + y;
            n++;
        }
        return n;
    }

    public static Complex CRef(double zoom, Complex offset) // used for high precision zooming
    {
        Complex cRef = offset;
        cRef = cRef / zoom;

        return cRef;
    }
}