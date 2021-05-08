#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLINCLUDE_INCLUDED

	struct SoundSource
	{
		float3 origin;
		float time;
		float duration;
	};

	uniform StructuredBuffer<SoundSource> _SoundSourcesBuffer : register(t1);
	uniform float _GameTime;
	uniform float _SoundCount;
	
	void MyFunction_float(float3 position, out float brightness)
	{
		int count = _SoundCount;
		float waveVelocity = 1;
		float waveBandWidthStart = 0.1;
		float waveBandWidthEnd = 1;
		
		brightness = 0;
		
		if(count == 0)
			return;
		
		for(int i = 0; i < count; i++)
		{
			SoundSource sound = _SoundSourcesBuffer[i];

			// Skip if the wave has expired
			if(_GameTime > sound.time + sound.duration)
				continue;

			float timeAlive = _GameTime - sound.time;
			float normalizedLifeTime = timeAlive / sound.duration;

			float band = lerp(waveBandWidthStart, waveBandWidthEnd, normalizedLifeTime);

			float waveEnd = timeAlive * waveVelocity;
			float waveStart = waveEnd - band;

			float distanceToPoint = distance(position, sound.origin);

			if (distanceToPoint > waveStart && distanceToPoint < waveEnd)
			{
				float normalizedWaveDistance = (distanceToPoint - waveStart) / band;
				float fade = sin(normalizedWaveDistance * 3.14);
				float value = saturate(fade - normalizedLifeTime);

				brightness = max(brightness, value);
			}
		}
	}

#endif