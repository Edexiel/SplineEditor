using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spline : MonoBehaviour
{
    public bool GameObjectHandle = false;

    private LineRenderer _lineRenderer = null;

    public enum SplineType
    {
        Bezier,
        Hermite,
        BSpline,
        CatmullRom
    }

    // [SerializeField] private bool _loop = false;
    // [SerializeField, HideInInspector] private bool _oldLoop = false;
    [SerializeField] private SplineType _splineType = SplineType.Bezier;
    [SerializeField, HideInInspector] private bool oldSmoothSpline = false;
    [SerializeField] public bool smoothSpline = false;
    [SerializeField] public List<Vector3> points;
    [SerializeField] private uint _lineSamples = 10;

    [HideInInspector] public bool isDirty; // when we have changed a point position we need to regen point line for line renderer

    private SplineType _oldSplineType = SplineType.Bezier; //to check if we changed line type so we can regen point list for line render
    // private List<Vector3> RenderPositions = new List<Vector3>();


    private void Awake()
    {
        _lineRenderer = gameObject.GetComponent<LineRenderer>();
    }

    public SplineType getSplineType()
    {
        return _splineType;
    }

    private void Start()
    {
        Debug.Assert(_lineRenderer != null, "There is no line renderer in this object");

        GenerateSpline();
    }

    private void Reset()
    {
        points = new List<Vector3>()
        {
            new Vector3(1f, 0f, 0f),
            new Vector3(2f, 0f, 0f),
            new Vector3(3f, 0f, 0f),
            new Vector3(4f, 0f, 0f)
        };
    }

    void Update()
    {
        //todo: draw thingy that moves on the spline
    }

    public int GetCurvesCount()
    {
        return points.Count / 3;
    }

    private void OnDrawGizmos()
    {
        // isDirty = true;

        if (_oldSplineType != _splineType)
        {
            isDirty = true;
            _oldSplineType = _splineType;
        }

        // if (_loop != _oldLoop)
        // {
        //     isDirty = true;
        //     _oldLoop = _loop;
        // }
        
        if (smoothSpline != oldSmoothSpline)
        {
            isDirty = true;
            oldSmoothSpline = smoothSpline;
        }

        if (isDirty)
        {
            GenerateSpline();
            isDirty = false;
        }
    }

    private void GenerateSpline()
    {
        if (_lineRenderer == null)
            _lineRenderer = gameObject.GetComponent<LineRenderer>();

        switch (_splineType)
        {
            case SplineType.Bezier:
                GenerateBezier();
                break;
            case SplineType.Hermite:
                GenerateHermite();
                break;
            case SplineType.BSpline:
                GenerateBSpline();
                break;
            case SplineType.CatmullRom:
                GenerateCatmullRom();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void GenerateBezier()
    {
        _lineRenderer.positionCount = (int) _lineSamples * points.Count;

        int nb = 0;
        for (int i = 3; i < points.Count; i += 3)
        {
            Vector3 p0 = points[i - 3];
            Vector3 p1 = points[i - 2]; //Point 1
            Vector3 p2 = points[i - 1]; //Point 2
            Vector3 p3 = points[i]; //End point

            float step = 1f / _lineSamples;
            for (float t = 0f; t < 1f; t += step)
            {
                _lineRenderer.SetPosition(nb++, GetBezierPoint(p0, p1, p2, p3, t));
            }

            _lineRenderer.SetPosition(nb++, GetBezierPoint(p0, p1, p2, p3, 1f));

            if(smoothSpline)
                SmoothCurve(i - 1);

        }

        _lineRenderer.positionCount = nb;
    }

    private Vector3 GetBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t * t * t;
        float invt = 1f - t;

        return Mathf.Pow(invt, 3) * p0
               + 3 * t * Mathf.Pow(invt, 2) * p1
               + 3 * t2 * (1f - t) * p2
               + t3 * p3;
    }

    private void GenerateHermite()
    {
        _lineRenderer.positionCount = (int) _lineSamples * points.Count;

        Vector3 p0 = points[0]; //Start point


        int nb = 0;
        for (int i = 1; i < points.Count; i += 3)
        {
            Vector3 p1 = points[i]; //Point 1
            Vector3 p2 = points[i + 1]; //Point 2
            Vector3 p3 = points[i + 2]; //End point

            //DrawBezier(p0, p3, p1, p2, SelectionColor, null, SelectionSize);
            float step = 1f / _lineSamples;
            for (float t = 0f; t < 1f; t += step)
            {
                _lineRenderer.SetPosition(nb++, GetHermitePoint(p0, p1, p2, p3, t));
            }

            _lineRenderer.SetPosition(nb++, GetHermitePoint(p0, p1, p2, p3, 1f));

            p0 = p3;
        }

        _lineRenderer.positionCount = nb;
    }

    private Vector3 GetHermitePoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t * t * t;

        return (2f * t3 - 3f * t2 + 1f) * p0 +
               (-2f * t3 + 3f * t2) * p3
               + (t3 - 2f * t2 + t) * p1
               + (t3 - t2) * p2;
    }

    private void GenerateBSpline()
    {
        //Start point
        _lineRenderer.positionCount = (int) _lineSamples * points.Count;

        int nb = 0;
        for (int i = 0; i < points.Count - 3; i++)
        {
            Vector3 p0 = points[i];
            Vector3 p1 = points[i + 1]; //Point 1
            Vector3 p2 = points[i + 2]; //Point 2
            Vector3 p3 = points[i + 3]; //End point

            float step = 1f / _lineSamples;
            for (float t = 0f; t < 1f; t += step)
            {
                _lineRenderer.SetPosition(nb++, GetBSplinePoint(p0, p1, p2, p3, t));
            }

            _lineRenderer.SetPosition(nb++, GetBSplinePoint(p0, p1, p2, p3, 1f));
        }

        _lineRenderer.positionCount = nb;
    }

    private Vector3 GetBSplinePoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t * t * t;

        return 1f / 6f *
               (Mathf.Pow(1 - t, 3) * p0
                + (3f * t3 - 6f * t2 + 4f) * p1
                + (-3f * t3 + 3f * t2 + 3f * t + 1f) * p2
                + t3 * p3
               );
    }


    private void GenerateCatmullRom()
    {
        //Start point
        _lineRenderer.positionCount = (int) _lineSamples * points.Count;

        int nb = 0;
        for (int i = 0; i < points.Count - 3; i++)
        {
            Vector3 p0 = points[i];
            Vector3 p1 = points[i + 1]; //Point 1
            Vector3 p2 = points[i + 2]; //Point 2
            Vector3 p3 = points[i + 3]; //End point

            float step = 1f / _lineSamples;
            for (float t = 0f; t < 1f; t += step)
            {
                _lineRenderer.SetPosition(nb++, GetCatmullPoint(p0, p1, p2, p3, t));
            }

            _lineRenderer.SetPosition(nb++, GetCatmullPoint(p0, p1, p2, p3, 1f));
            

        }

        _lineRenderer.positionCount = nb;
    }

    private Vector3 GetCatmullPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t * t * t;

        return 0.5f * (
            (t3 - t2) * p3 +
            (-3f * t3 + 4f * t2 + t) * p2 +
            (3f * t3 - 5f * t2 + 2f) * p1 +
            (-t3 + 2f * t2 - t) * p0
        );
    }

    public void AddCurve()
    {
        Vector3 point = points[points.Count - 1];
        Vector3 startPoint = points[points.Count - 4];
        Vector3 direction = Vector3.Normalize(point - startPoint) * 10;
        point += direction;
        points.Add(point);
        point += direction;
        points.Add(point);
        point += direction;
        points.Add(point);
    }

    public void DeleteCurve(int index)
    {
    }

    public void DeleteLastCurve()
    {
        points.RemoveRange(points.Count - 4, 3);
    }

    public void SmoothCurve(int index)
    {
        int maxIndex = points.Count - 1;

        if ((index + 1) % 3 == 0 && (index + 2) < maxIndex)
        {
            points[index + 2] = points[index + 1] - (points[index] - points[index + 1]);
        }
        else if ((index - 1) % 3 == 0 && (index - 2) >= 0)
        {
            points[index - 2] = points[index - 1] - (points[index] - points[index - 1]);
        }
    }
}