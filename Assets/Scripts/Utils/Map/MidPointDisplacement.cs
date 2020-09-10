using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public static class MidpointDisplacement
    {
	    private static int _n;
	    private static float _spread;
	    private static float _spreadReductionRate;
	    private static int _edgeLength;
	    private static float[,] _heightmap;
	    public static float[,] CreateHeightmap(int n, int seed, float spread, float spreadReductionRate)
	    {
		    _n = n;
		    _spread = spread;
		    _spreadReductionRate = spreadReductionRate;
		    
		    Random.InitState(seed);
		    _edgeLength = CalculateLength(n);
		    _heightmap = new float[_edgeLength, _edgeLength];
		    ClearHeightmap();
		    RandomiseCorners();
		    MDisplacement();
		    NormaliseHeightmap();

		    return _heightmap;
	    }

	    private static int CalculateLength(int n)
	    {
		    return (int)Mathf.Pow(2, n) +1;
	    }

	    private static void ClearHeightmap()
	    {
		    for(int y = 0; y < _edgeLength; y++)
		    {
			    for(int x = 0; x < _edgeLength; x++)
			    {
				    _heightmap[x,y] = 0.0f;
			    }
		    }
	    }

	    private static void NormaliseHeightmap()
	    {
		    float min = float.MaxValue;
		    float max = float.MinValue;

		    for(int y = 0; y < _edgeLength; y++)
		    {
			    for(int x = 0; x < _edgeLength; x++)
			    {
				    float current = _heightmap[x,y];
				    if(current < min)
					    min = current;
				    else if(current > max)
					    max = current;
			    }
		    }

		    for(int y = 0; y < _edgeLength; y++)
		    {
			    for(int x = 0; x < _edgeLength; x++)
			    {
				    _heightmap[x,y] = Mathf.InverseLerp(min, max, _heightmap[x,y]);
			    }
		    }
	    }

	    private static void RandomiseCorners()
	    {
		    _heightmap[0, 0] = GetRandom();
		    _heightmap[0, _edgeLength-1] = GetRandom();
		    _heightmap[_edgeLength-1, 0] = GetRandom();
		    _heightmap[_edgeLength-1, _edgeLength-1] = GetRandom();
	    }

	    private static float GetRandom()
	    {
		    return Random.Range(-1.0f, 1.0f);
	    }

	    private static float GetOffset()
	    {
		    return GetRandom() * _spread;
	    }

	    private static int GetMidpoint(int a, int b)
	    {
		    return a+((b-a)/2);
	    }

	    private static float GetAverageOf2(float a, float b)
	    {
		    return (a+b)/2.0f;
	    }

	    private static float GetAverageOf4(float a, float b, float c, float d)
	    {
		    return (a+b+c+d)/4.0f;
	    }

	    private static void MDisplacement()
	    {
		    int i = 0;
		    while (i < _n)
		    {
			    int numberOfQuads = (int)Mathf.Pow(4, i);
			    int quadsPerRow = (int)Mathf.Sqrt(numberOfQuads);
			    int quadLength = (_edgeLength-1)/quadsPerRow;

			    for(int y = 0; y < quadsPerRow; y++)
			    {
				    for(int x = 0; x < quadsPerRow; x++)
				    {
					    CalculateMidpoints(quadLength*x, quadLength*(x+1), quadLength*y, quadLength*(y+1));
				    }
			    }
			    _spread *= _spreadReductionRate;
			    i++;
		    }
	    }

	    private static void CalculateMidpoints(int x0, int x1, int y0, int y1)
	    {
		    int mx = GetMidpoint(x0, x1);
		    int my = GetMidpoint(y0, y1);
		    float bottom = _heightmap[mx, y0] = GetAverageOf2(_heightmap[x0, y0], _heightmap[x1,y0]) + GetOffset();
		    float top = _heightmap[mx, y1] = GetAverageOf2(_heightmap[x0, y1], _heightmap[x1,y1]) + GetOffset();
		    float left = _heightmap[x0, my] = GetAverageOf2(_heightmap[x0, y0], _heightmap[x0,y1]) + GetOffset();
		    float right = _heightmap[x1, my] = GetAverageOf2(_heightmap[x1, y0], _heightmap[x1,y1]) + GetOffset();
		    _heightmap[mx, my] = GetAverageOf4(bottom, top, left, right) + GetOffset();
	    }
    }

}
