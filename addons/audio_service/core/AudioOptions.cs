namespace AudioService;

public readonly record struct AudioOptions(
    float VolumeDb = 0f, float PitchScale = 1f, float PitchVariance = 0f);
