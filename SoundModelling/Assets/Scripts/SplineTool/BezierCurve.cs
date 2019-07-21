using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CurveMode
{
    Free,
    Align,
    Mirror
}

public class BezierCurve : Line
{

    [SerializeField]
    private List<CurveMode> modes;

    private bool loop;

    public bool Loop
    {
        get
        {
            return loop;
        }
        set
        {
            loop = value;
            if (loop)
            {
                modes[modes.Count - 1] = modes[0];
                points[0] = points[points.Count - 1];
                EnforceMode(0);
            }
        }
    }

    public override void Reset()
    {
        base.Reset();

        modes = new List<CurveMode>
        {
            CurveMode.Free,
            CurveMode.Free,
        };
    }

    public int PointCount
    {
        get
        {
            return points.Count;
        }
    }

    public void SetControlPoint(int index, Vector3 point)
    {
        if (index % 3 == 0)
        {
            Vector3 delta = point - points[index];
            if (index > 0)
            {
                points[index - 1] += delta;
            }
            if (index + 1 < points.Count)
            {
                points[index + 1] += delta;
            }
            if (loop && (index == 0 || index == PointCount - 1))
            {
                if (index == 0)
                {
                    points[PointCount - 1] += delta;
                }
                else
                {
                    points[0] += delta;
                }
            }
        }

        points[index] = point;
        EnforceMode(index);
    }

    public Vector3 GetControlPoint(int index)
    {
        return points[index];
    }

    public void AddControlPoint(Vector3 point)
    {
        points.Add(point);
    }

    public CurveMode GetControlPointMode(int index)
    {
        int modeIndex = (index + 1) / 3;
        return modes[modeIndex];
    }

    public void SetControlPointMode(int index, CurveMode m)
    {
        int modeIndex = (index + 1) / 3;
        modes[modeIndex] = m;
        // need to make sure if loop is on, the first and last point agrees on the mode
        if (loop)
        {
            if (modeIndex == 0)
            {
                modes[modes.Count - 1] = m;
            }
            else if (modeIndex == modes.Count - 1)
            {
                modes[0] = m;
            }
        }
        EnforceMode(index);
    }

    public void AddControlPointMode()
    {
        modes.Add(modes[modes.Count - 1]);
    }

    public void EnforceMode(int index)
    {
        int modeIndex = (index + 1) / 3;
        CurveMode mode = modes[modeIndex];
        if (mode == CurveMode.Free || !loop && (modeIndex == 0 || modeIndex == modes.Count - 1))
        {
            return;
        }

        int middleIndex = modeIndex * 3;
        int fixedIndex, enforcedIndex;
        if (index <= middleIndex)
        {
            fixedIndex = middleIndex - 1;
            if (fixedIndex < 0)
            {
                fixedIndex = PointCount - 2;
            }
            enforcedIndex = middleIndex + 1;
            if (enforcedIndex >= PointCount)
            {
                enforcedIndex = 1;
            }
        }
        else
        {
            fixedIndex = middleIndex + 1;
            if (fixedIndex >= PointCount)
            {
                fixedIndex = 1;
            }
            enforcedIndex = middleIndex - 1;
            if (enforcedIndex < 0)
            {
                enforcedIndex = PointCount - 2;
            }
        }

        Vector3 middle = points[middleIndex];
        Vector3 tangent = middle - points[fixedIndex];
        if (mode == CurveMode.Align)
        {
            tangent = tangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
        }
        points[enforcedIndex] = points[middleIndex] + tangent;
    }

    public Vector3 GetBezierPoint(float t)
    {
        int i;
        t = Mathf.Clamp01(t);
        if (t == 1)
        {
            i = points.Count - 4;
        }
        else
        {
            t = t * CurveCount;
            i = (int)t;
            t = t - i;
            i = i * 3;
        }

        return transform.TransformPoint(Bezier.GetPoint(points[i], points[i+1], points[i+2], points[i+3], t));
    }

    public Vector3 GetTangent(float t)
    {
        int i;
        t = Mathf.Clamp01(t);
        if (t == 1)
        {
            i = points.Count - 4;
        }
        else
        {
            t = t * CurveCount;
            i = (int)t;
            t = t - i;
            i = i * 3;
        }

        return transform.TransformPoint(Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t)) - transform.position;
    }

    public Vector3 GetDirection(float t)
    {
        return GetTangent(t).normalized;
    }

    public void AddCurve()
    {
        Vector3 point = GetControlPoint(PointCount - 1);
        point += new Vector3(1, 0, 1);
        AddControlPoint(point);
        point += new Vector3(1, 0, 1);
        AddControlPoint(point);
        point += new Vector3(1, 0, 1);
        AddControlPoint(point);

        AddControlPointMode();

        EnforceMode(PointCount - 4);

        if (Loop)
        {
            points[0] = points[PointCount - 1];
            modes[0] = modes[modes.Count - 1];
            EnforceMode(0);
        }
    }

    public int CurveCount
    {
        get
        {
            return (points.Count - 1) / 3;
        }
    }
}
