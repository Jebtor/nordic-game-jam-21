#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLINCLUDE_INCLUDED

	struct SoundSource
	{
		float3 origin;
		float time;
	};

	uniform StructuredBuffer<SoundSource> _SoundSourcesBuffer : register(t1);
	uniform float _GameTime;
	uniform float _SoundCount;
	
	void MyFunction_float(float3 position, out float brightness)
	{
		int count = _SoundCount;
		float waveVelocity = 1;
		float waveBandWidth = 0.2;	
		
		brightness = 0;
		
		if(count == 0)
			return;
		
		for(int i = 0; i < count; i++)
		{
			SoundSource sound = _SoundSourcesBuffer[i];

			float dist = distance(position, sound.origin) * waveVelocity;
			float waveDistance = _GameTime - sound.time;
			
			if(dist > waveDistance - waveBandWidth && dist < waveDistance)
				brightness = max(brightness, 1);
		}
	}

#endif