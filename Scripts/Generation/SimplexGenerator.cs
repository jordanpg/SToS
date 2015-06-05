using UnityEngine;
using System.Collections;

public class SimplexGenerator : MonoBehaviour
{
	public enum MaskMode
	{
		None,
		Linear,
		Cosine,
		SmoothStep
	}

	public float seed = 0f;
	public bool randomiseSeed = false;

	public float Width = 512;
	public float Height = 512;
	private float hWidth;
	private float hHeight;
	
	public bool regen = false;
	public float frequency = 640f;
	public float persistence = 0.6f;
	public int iterations = 8;

	public Color deepWater = new Color(0f, 0f, 1f);
	public Color water = new Color(0f, 0.2f, 0.8f);
	public Color land = new Color(0f, 1f, 0f);
	public Color sand = new Color(0.82f, 0.78f, 0.63f);
	public bool sandAsBorder = false;

	public Color waterDeepest = new Color(0f, 0f, 1f);
	public Color waterShallowest = new Color(0f, 0.3f, 0.7f);
	public Color landLowest = new Color(0f, 0.36f, 0f);
	public Color landHighest = new Color(0f, 0.65f, 0f);

	public float deepSeaLevel = -0.2f;
	public float seaLevel = 0f;
	public float beachExtent = 0.02f;
	public float tidePercentage = 0.25f;

	public MaskMode mask = MaskMode.Linear;
	private float refDist = 0f;
	public float edgeVal = -1f;
	public bool fromCentre = true;

	private Texture2D noise;
	private Color[] pix;
	private Renderer rend;

	public static float cosineInterpolate(float from, float to, float mu)
	{
		float mu2 = (1 - Mathf.Cos(mu * Mathf.PI)) / 2;
		return (from * (1 - mu2) + to * mu2);
	}

	// Use this for initialization
	void Start()
	{
		rend = GetComponent<Renderer>();

		CalcNoise();
	}

	float getScaledValue(float x, float a0, float b0, float a1, float b1)
	{
		return a1 + (x - a0) * (b1 - a1) / (b0 - a0);
	}

	float getHeight(float x, float y, float seed, int iter)
	{
		float maxAmp = 0f;
		float amp = 1f;
		float f = 1f / frequency;
		float noise = 0f;

		for(int i = 0; i < iter; i++)
		{
			noise += Simplex.noise(x * f, y * f, seed) * amp;
			maxAmp += amp;
			amp *= persistence;
			f *= 2f;
		}

		return noise / maxAmp;
	}

	float applyMask(float height, float x, float y, float seed)
	{
		if(mask == MaskMode.None)
			return height;

		float amt = Mathf.Sqrt(Mathf.Pow(hWidth - x, 2) + Mathf.Pow(hHeight - y, 2)) / refDist;

		switch(mask)
		{
			case MaskMode.Linear:
				if(fromCentre)
					return Mathf.Lerp(height, edgeVal, amt);
				else
					return Mathf.Lerp(edgeVal, height, amt);
			case MaskMode.Cosine:
				if(fromCentre)
					return SimplexGenerator.cosineInterpolate(height, edgeVal, amt);
				else
					return SimplexGenerator.cosineInterpolate(edgeVal, height, amt);
			case MaskMode.SmoothStep:
				if(fromCentre)
					return Mathf.SmoothStep(height, edgeVal, amt);
				else
					return Mathf.SmoothStep(edgeVal, height, amt);
			default:
				return height;
		}
	}

	Color getAltColour(float height)
	{
		if(height < (seaLevel + deepSeaLevel))
			return deepWater;
		else if(height < seaLevel)
			return water;
		else if(height < (seaLevel + beachExtent))
			return sand;
		else
			return land;
	}

	Color getDynamicColour(float height)
	{
		float tideExtent = (beachExtent * tidePercentage);
		if(height < seaLevel - tideExtent)
		{
			float prog = getScaledValue(height, -1f, seaLevel, 0f, 1f);
			return Color.Lerp(waterDeepest, waterShallowest, prog);
		}
		else if(height < seaLevel + tideExtent)
		{
			if(!sandAsBorder)
			{
				float prog = getScaledValue(height, seaLevel - tideExtent, seaLevel + tideExtent, 0f, 1f);
				return Color.Lerp(waterShallowest, sand, prog);
			}
			else
				return sand;
		}
		else if(height < seaLevel + beachExtent)
		{
			if(!sandAsBorder)
			{
				float prog = getScaledValue(height, seaLevel, seaLevel + beachExtent, 0f, 1f);
				return Color.Lerp(sand, landLowest, prog);
			}
			else
				return sand;
		}
		else
		{
			float prog = getScaledValue(height, seaLevel + beachExtent, 1f, 0f, 1f);
			return Color.Lerp(landLowest, landHighest, prog);
		}
	}

	void CalcNoise()
	{
		if(noise != null)
			Destroy(noise);
		noise = new Texture2D((int)Width, (int)Height);
		rend.material.mainTexture = noise;

		pix = new Color[noise.width * noise.height];

		hWidth = Width / 2f;
		hHeight = Height / 2f;

		refDist = Mathf.Sqrt(Mathf.Pow((hWidth - Width), 2) + Mathf.Pow((hHeight - Height), 2));

		regen = false;

		if(randomiseSeed)
			seed = Random.Range((float)0x0, (float)0xFFFF);

		for(float y = 0f; y < noise.height; y++)
		{
			for(float x = 0f; x < noise.width; x++)
			{
				float samp = applyMask(getHeight(x, y, seed, iterations), x, y, seed);
				//Debug.Log(samp.ToString() + " X: " + x.ToString() + " Y: " + y.ToString() + " Seed: " + seed.ToString());

				//Debug.Log("Fixed: " + samp.ToString());
				pix[(int)(y * noise.width + x)] = getDynamicColour(samp);
				//yield return null;
			}
		}
		noise.SetPixels(pix);
		noise.Apply();
	}

	// Update is called once per frame
	void Update()
	{
		if(regen)
			CalcNoise();
	}
}
