using UnityEngine;
using System.Collections;

public class Simplex
{
	private static readonly Vector3[] grad3 = {new Vector3(1f, 1f, 0f), new Vector3( -1f, 1f, 0f), new Vector3(1f, -1f, 0), new Vector3(-1f,-1f, 0f),
																	new Vector3(1f, 0f, 1f), new Vector3(-1f, 0f, 1f), new Vector3(1f, 0f, -1f), new Vector3(-1f, 0f, -1f),
																	new Vector3(0f, 1f, 1f), new Vector3(0f, -1f, 1f), new Vector3(0f, 1f, -1f), new Vector3(0f, -1f, -1f)};

	private static readonly int[] p = {151, 160, 137, 91, 90, 15,
																		131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23,
																		190, 6, 148, 247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177, 33,
																		88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165, 71, 134, 139, 48, 27, 166,
																		77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244,
																		102, 143, 54, 65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 200, 196,
																		135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64, 52, 217, 226, 250, 124, 123,
																		5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42,
																		223, 183, 170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9,
																		129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104, 218, 246, 97, 228,
																		251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107,
																		49, 192, 214, 31, 181, 199, 106, 157, 184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254,
																		138, 236, 205, 93, 222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180};
	private static readonly int[] perm = new int[512];
	private static readonly int[] perm12 = new int[512];

	private static readonly float F2, G2, F3, G3;

	public static int fastFloor(float x)
	{
		int xi = (int)x;
		return x < xi ? xi - 1 : xi;
	}

	static Simplex()
	{
		int j;
		for(int i = 0; i < 512; i++)
		{
			j = i & 255;
			perm[i] = p[j];
			perm12[i] = perm[j] % 12;
		}

		float sq3 = Mathf.Sqrt(3f);
		F2 = 0.5f * (sq3 - 1f);
		G2 = (3f - sq3) / 6f;
		F3 = 1f / 3f;
		G3 = 1f / 6f;
	}

	private static float dot(Vector3 grad, float x, float y, float z)
	{
		return grad.x * x + grad.y * y + grad.z * z;
	}

	private static float dot(Vector3 grad, float x, float y)
	{
		return grad.x * x + grad.y * y;
	}

	public static float noise(float xi, float yi, float zi)
	{
		float n0, n1, n2, n3;

		float s = (xi + yi + zi) * F3;
		int i = fastFloor (xi + s);
		int j = fastFloor (yi + s);
		int k = fastFloor (zi + s);
		float t = (i + j + k) * G3;

		float X0 = (float)i - t;
		float Y0 = (float)j - t;
		float Z0 = (float)k - t;
		float x0 = xi - X0;
		float y0 = yi - Y0;
		float z0 = zi - Z0;

		int i1, j1, k1, i2, j2, k2;
		if(x0 >= y0)
		{
			if(y0 > z0)
			{
				i1 = 1;  j1 = 0; k1 = 0; i2 = 1; j2 = 1; k2 = 0;
			}
			else if(x0 >= z0)
			{
				i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 0; k2 = 1;
			}
			else
			{
				i1 = 0; j1 = 0; k1 = 1; i2 = 1; j2 = 0; k2 = 1;
			}
		}
		else
		{
			if(y0<z0)
			{
				i1 = 0; j1 = 0; k1 = 1; i2 = 0; j2 = 1; k2 = 1;
			}
			else if(x0<z0) 
			{ 
				i1 = 0; j1 = 1; k1 = 0; i2 = 0; j2 = 1; k2 = 1; 
			}
			else 
			{ 
				i1 = 0; j1 = 1; k1 = 0; i2 = 1; j2 = 1; k2 = 0; 
			}
		}

		float x1 = x0 - (float)i1 + G3; 
		float y1 = y0 - (float)j1 + G3;
		float z1 = z0 - (float)k1 + G3;
		float x2 = x0 - (float)i2 + 2.0f * G3; 
		float y2 = y0 - (float)j2 + 2.0f * G3;
		float z2 = z0 - (float)k2 + 2.0f * G3;
		float x3 = x0 - 1.0f + 3.0f * G3; 
		float y3 = y0 - 1.0f + 3.0f * G3;
		float z3 = z0 - 1.0f + 3.0f * G3;

		int ii = i & 255;
		int jj = j & 255;
		int kk = k & 255;
		int gi0 = perm12[ii+perm[jj+perm[kk]]];
		int gi1 = perm12[ii+i1+perm[jj+j1+perm[kk+k1]]];
		int gi2 = perm12[ii+i2+perm[jj+j2+perm[kk+k2]]];
		int gi3 = perm12[ii+1+perm[jj+1+perm[kk+1]]];

		float t0 = 0.6f - x0 * x0 - y0 * y0 - z0 * z0;
		if (t0 < 0)
			n0 = 0.0f;
		else 
		{
			t0 *= t0;
			n0 = t0 * t0 * dot (grad3 [gi0], x0, y0, z0);
		}

		float t1 = 0.6f - x1 * x1 - y1 * y1 - z1 * z1;
		if (t1 < 0)
			n1 = 0.0f;
		else
		{
			t1 *= t1;
			n1 = t1 * t1 * dot (grad3 [gi1], x1, y1, z1);
		}

		float t2 = 0.6f - x2 * x2 - y2 * y2 - z2 * z2;
		if (t2 < 0)
			n2 = 0.0f;
		else
		{
			t2 *= t2;
			n2 = t2 * t2 * dot (grad3 [gi2], x2, y2, z2);
		}

		float t3 = 0.6f - x3 * x3 - y3 * y3 - z3 * z3;
		if (t3 < 0)
			n3 = 0.0f;
		else 
		{
			t3 *= t3;
			n3 = t3 * t3 * dot (grad3 [gi3], x3, y3, z3);
		}

		return 32f * (n0 + n1 + n2 + n3);
	}

	public static float noise(float xi, float yi)
	{
		float n0, n1, n2;

		float s = (xi + yi) * F2;
		int i = fastFloor (xi + s);
		int j = fastFloor(yi + s);
		float t = (i + j) * G2;
		float X0 = i - t;
		float Y0 = j - t;
		float x0 = xi - X0;
		float y0 = yi - Y0;

		int i1, j1;
		if(x0 > y0)
		{
			i1 = 1; j1 = 0;
		}
		else
		{
			i1 = 0; j1 = 1;
		}

		float x1 = x0 - i1 + G2;
		float y1 = y0 - j1 + G2;
		float x2 = x0 - 1.0f + 2.0f * G2;
		float y2 = y0 - 1.0f + 2.0f * G2;

		int ii = i & 255;
		int jj = j & 255;
		int gi0 = perm12 [ii + perm [jj]];
		int gi1 = perm12 [ii + i1 + perm [jj + j1]];
		int gi2 = perm12 [ii + 1 + perm [jj + 1]];

		float t0 = 0.5f - x0 * x0 - y0 * y0;
		if (t0 < 0)
			n0 = 0.0f;
		else 
		{
			t0 *= t0;
			n0 = t0 * t0 * dot(grad3[gi0], x0, y0); 
		}

		float t1 = 0.5f - x1 * x1 - y1 * y1;
		if (t1 < 0)
			n1 = 0.0f;
		else 
		{
			t1 *= t1;
			n1 = t1 * t1 * dot(grad3[gi1], x1, y1);
		}

		float t2 = 0.5f - x2 * x2 - y2 * y2;
		if (t2 < 0)
			n2 = 0.0f;
		else 
		{
			t2 *= t2;
			n2 = t2 * t2 * dot(grad3[gi2], x2, y2);
		}

		return 70f * (n0 + n1 + n2);
	}

	public static float fitFloat(float noise, float min, float max)
	{
		return noise * (max - min) / 2f + (max + min) / 2f;
	}
}
