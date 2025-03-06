
float SmoothingKernelPoly6(float dst, float radius, float val)
{
	if (dst < radius)
	{
		float v = radius * radius - dst * dst;
		return v * v * v * val;
	}
	return 0;
}

float SpikyKernelPow3(float dst, float radius, float val)
{
	if (dst < radius)
	{
		float v = radius - dst;
		return v * v * v * val;
	}
	return 0;
}

float SpikyKernelPow2(float dst, float radius, float val)
{
	if (dst < radius)
	{
		float v = radius - dst;
		return v * v * val;
	}
	return 0;
}

float DerivativeSpikyPow3(float dst, float radius, float val)
{
	if (dst <= radius)
	{
		float v = radius - dst;
		return -v * v * val;
	}
	return 0;
}

float DerivativeSpikyPow2(float dst, float radius, float val)
{
	if (dst <= radius)
	{
		float v = radius - dst;
		return -v * val;
	}
	return 0;
}
