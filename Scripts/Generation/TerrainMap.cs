using UnityEngine;
using System.Collections;

public class TerrainMap
{
	private float[,] map;
	private int width, height, len;

	public float getVal(int x, int y)
	{
		if(map == null)
			return 0f;

		if(x >= width || y >= height)
			return 0f;

		return map[x, y];
	}

	public void setVal(int x, int y, float val)
	{
		if(map == null)
			return;

		map[x, y] = val;
	}

	public void createMap(int _width, int _height)
	{
		map = new float[_width, _height];
		width = _width;
		height = _height;
		len = width * height;
	}

	public void setMap(float[,] _map)
	{
		map = _map;
		width = _map.GetLength(0);
		height = _map.GetLength(1);
		len = width * height;
	}

	public void setMapData(float[] _map, int width)
	{
		if(map == null)
			return;

		if(_map.Length < len)
			return;

		int i = 0;
		int y;
		foreach(float h in _map)
		{
			y = i / width;
			map[y + (i % width), y] = h;
			i++;
		}
	}
}
