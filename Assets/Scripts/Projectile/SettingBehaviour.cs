using UnityEngine;

[DisallowMultipleComponent]
public class SettingBehaviour : MonoBehaviour
{
    [SerializeField] AnimationCurveFunction _animationCurveFunctions;
    [SerializeField] ProjectileMovement _projectile;

    [Space]
    [Header("Curve")]
    [SerializeField] AnimationCurveType _curveType;
    [SerializeField] TimingBehaviour _timingBehaviour;

    [Space]
    [Header("Behaviour")]
    [SerializeField] [Min(0.1f)] float _period;
    [SerializeField] float _emphasize;
    [SerializeField] bool _periodDependsOnRhythm;
    [SerializeField] bool _smoothPeriodChanges;

    [Space]
    [Header("Settings to affect")]
    [SerializeField] bool _affectSpeed;
    [SerializeField] bool _affectDirection;
    [SerializeField] bool _affectSize;

    AnimationCurve _curve;
    float _elapsedTime;
    float _previousPeriod = 1;


    public AnimationCurveFunction AnimationCurveFunctions
    {
        get 
        {
            if (!_animationCurveFunctions)
                _animationCurveFunctions = AnimationCurveFunction.Instance;

            return _animationCurveFunctions;
        }
        set { _animationCurveFunctions = value; }
    }
    public ProjectileMovement Projectile
    {
        get 
        {
            if (_projectile)
                return _projectile;
            else
                Debug.LogWarning("The Projectile Movement reference in Setting Behaviour script is missing.", this);
            return null;
        }
        set { _projectile = value; }
    }
    public AnimationCurveType CurveType
    {
        get { return _curveType; }
        set { _curveType = value; }
    }
    public float Period
    {
        get { return _period; }
        set { _period = value; }
    }
    public float Emphasize
    {
        get { return _emphasize; }
        set { _emphasize = value; }
    }
    public bool AffectSpeed
    {
        get { return _affectSpeed; }
        set { _affectSpeed = value; }
    }
    public bool AffectSize
    {
        get { return _affectSize; }
        set { _affectSize = value; }
    }
    public bool AffectDirection
    {
        get { return _affectDirection; }
        set { _affectDirection = value; }
    }
    public bool PeriodDependsOnRhythm
    {
        get { return _periodDependsOnRhythm; }
        set { _periodDependsOnRhythm = value; }
    }
    public bool SmoothPeriodChanges
    {
        get { return _smoothPeriodChanges; }
        set { _smoothPeriodChanges = value; }
    }
    public TimingBehaviour TimingBehaviour
    {
        get { return _timingBehaviour; }
        set { _timingBehaviour = value; }
    }

    void Awake()
    {
        if(!_animationCurveFunctions)
            _animationCurveFunctions = AnimationCurveFunction.Instance;

        if (!_projectile)
            _projectile = GetComponent<ProjectileMovement>();
    }

    void Update()
    {
        if (!_animationCurveFunctions)
        {
            Debug.LogWarning("The Projectile Movement reference in Setting Behaviour script is missing.");
            return;
        }
        _curve = _animationCurveFunctions.GetCurve(_curveType);
        SetLerpFunction();
    }

    public void SetSpeedBehaviour(SettingBehaviour sb)
    {
        _period = sb.Period;
        _curveType = sb.CurveType;
        _emphasize = sb.Emphasize;
        _timingBehaviour = sb.TimingBehaviour;
        _affectDirection = sb.AffectDirection;
        _affectSpeed = sb.AffectSpeed;
        _affectSize = sb.AffectSize;
        _periodDependsOnRhythm = sb.PeriodDependsOnRhythm;
        _smoothPeriodChanges = sb.SmoothPeriodChanges;
    }

    void SetLerpFunction()
    {
        if (_previousPeriod != _period)
            _previousPeriod = _period;

        _period = GetPeriodOnRhythm();

        if (_period < .1f)
            _period = .1f;

        _elapsedTime = ClampTimeToPeriod(CalculateTimingBehaviour(_timingBehaviour));

        float lerpRatio = _elapsedTime / _period;
        AffectChosenSetting(_curve.Evaluate(lerpRatio) * _emphasize);
    }

    void AffectChosenSetting(float curveValue)
    {
        if (_affectSpeed) { _projectile.MoveSpeed *= curveValue; }
        if(_affectDirection) { _projectile.Direction *= curveValue; }
        if (_affectSize) { _projectile.Size *= curveValue; }
    }

    float GetPeriodOnRhythm()
    {
        RecordingPlayer rhythm = _projectile.ProjectileController.Rhythm;

        if (!rhythm) { 
            Debug.LogWarning("Rhythm reference in projectile controller script is missing."); 
            return _period;
        }

        MouseClickingRecorder.Recording recording = rhythm.recordingDictionary[rhythm.recordingTagToPlay];

        if (_periodDependsOnRhythm && recording.index < recording.timeBetweenClicks.Count - 1)
            return (float)recording.timeBetweenClicks[recording.index];
        else
            return _period;
    }

    float CalculateTimingBehaviour(TimingBehaviour behaviourName)
    {
        return behaviourName switch
        {
            TimingBehaviour.ShiftedTime => Time.time + _projectile.AppearanceTime,
            TimingBehaviour.RealTime => Time.time,
            TimingBehaviour.EachTime => _projectile.AppearanceTime,
            _ => default,
        };
    }

    float ClampTimeToPeriod(float time)
    {
        float currentPeriod = _smoothPeriodChanges ? CalculateSmoothPeriodChange() : _period;
        float clampedTime = time / currentPeriod % 1;
        return clampedTime;
    }

    float CalculateSmoothPeriodChange()
    {
        float velocity = 0.0f;
        float smoothedPeriod = Mathf.SmoothDamp(_previousPeriod, _period, ref velocity, _previousPeriod);
        return smoothedPeriod;
    }
}
