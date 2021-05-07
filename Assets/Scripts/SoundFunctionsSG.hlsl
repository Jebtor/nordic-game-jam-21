#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLINCLUDE_INCLUDED

	uniform StructuredBuffer<float3> _SoundOriginsBuffer : register(t1);
	uniform StructuredBuffer<float> _SoundTimesBuffer : register(t2);
	uniform float _GameTime;
	uniform float _SoundsCount;
	
	void MyFunction_float(float3 position, out float brightness)
	{
		int count = (int)_SoundsCount;
		float waveVelocity = 1;
		float waveBandWidth = 0.2;	
		
		brightness = 0;
		
		if(count == 0)
			return;
		
		for(int i = 0; i < count; i++)
		{
			float3 origin = _SoundOriginsBuffer[i];
			float dist = distance(position, origin) * waveVelocity;
			
			float startTime = _SoundTimesBuffer[i];
			float waveDistance = _GameTime - startTime;
			
			if(dist > waveDistance - waveBandWidth && dist < waveDistance)
				brightness = max(brightness, 1);
		}
	}

#endif