using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tube : MonoBehaviour
{
    public Color[] waterColors;
    public SpriteRenderer waterSprite;

    public float timeToRotate = 1f;
    
    public AnimationCurve scaleAndRotationMultiplierCurve;
    public AnimationCurve fillAmountCurve;
    public AnimationCurve rotationSpeedCurve;
    
    public float[] fillAmounts;
    public float[] rotationValues;

    protected int rotationIndex = 0;
    [Range(0, 4)]
    public int numberOfWaters = 5;
    public Color topColor;
    public int numberOfTopColorLayers = 1;

    public Tube tubeRef;
    public bool justThisTube = false;
    protected int numberOfWatersToPour = 0;

    public Transform leftRotationPoint;
    public Transform rightRotationPoint;
    protected Transform choseRotationPoint;
    
    protected float directionMultiplier = 1f;
    
    protected Vector3 originalPos;
    protected Vector3 startPos;
    protected Vector3 endPos;

    public LineRenderer lr;

    protected void Start()
    {
        waterSprite.material.SetFloat("_FillAmount", fillAmounts[numberOfWaters]);
        
        originalPos = transform.position;
        UpdateColorsOnShader();
        UpdateTopWaterColor();
    }

    protected void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && justThisTube)
        {
            UpdateTopWaterColor();
            if (tubeRef.FillTubeCheck(topColor))
            {
                ChooseRotationPointAndDirection();
                numberOfWatersToPour = Mathf.Min(numberOfTopColorLayers, 4 - tubeRef.numberOfWaters);
                for (int i = 0; i < numberOfWatersToPour; i++)
                {
                    tubeRef.waterColors[tubeRef.numberOfWaters + i] = topColor;
                }
                tubeRef.UpdateColorsOnShader();
            }
            CalculateRotationIndex(4 - tubeRef.numberOfWaters);
            StartCoroutine(RotateTube());
        }
    }

    public void StartWaterPouring()
    {
        ChooseRotationPointAndDirection();
        numberOfWatersToPour = Mathf.Min(numberOfTopColorLayers, 4 - tubeRef.numberOfWaters);
        for (int i = 0; i < numberOfWatersToPour; i++)
        {
            tubeRef.waterColors[tubeRef.numberOfWaters + i] = topColor;
        }
        tubeRef.UpdateColorsOnShader();
        
        CalculateRotationIndex(4 - tubeRef.numberOfWaters);
        StartCoroutine(MoveTube());
    }

    public void UpdateColorsOnShader()
    {
        waterSprite.material.SetColor("_Color01", waterColors[0]);
        waterSprite.material.SetColor("_Color02", waterColors[1]);
        waterSprite.material.SetColor("_Color03", waterColors[2]);
        waterSprite.material.SetColor("_Color04", waterColors[3]);
    }

    protected IEnumerator RotateTube()
    {
        float t = 0;
        float lerpValue;
        float angleValue;
        
        float lastAngleValue = 0;

        while (t < timeToRotate)
        {
            lerpValue = t / timeToRotate;
            angleValue = Mathf.Lerp(0, directionMultiplier * rotationValues[rotationIndex], lerpValue);
            
            transform.RotateAround(choseRotationPoint.position, Vector3.forward, lastAngleValue - angleValue);
            waterSprite.material.SetFloat("_ScaleAndRotationMultiplied", scaleAndRotationMultiplierCurve.Evaluate(angleValue));
            
            if(fillAmounts[numberOfWaters] > fillAmountCurve.Evaluate(angleValue) + 0.005f)
            {
                if (!lr.enabled)
                {
                    lr.startColor = topColor;
                    lr.endColor = topColor;
                    lr.SetPosition(0, choseRotationPoint.position);
                    lr.SetPosition(1, choseRotationPoint.position - Vector3.up * 1.45f);
                    lr.enabled = true;
                }
                waterSprite.material.SetFloat("_FillAmount", fillAmountCurve.Evaluate(angleValue));
                tubeRef.FillUpTube(fillAmountCurve.Evaluate(lastAngleValue) - fillAmountCurve.Evaluate(angleValue));
            }
            
            t += Time.deltaTime * rotationSpeedCurve.Evaluate(angleValue);
            lastAngleValue = angleValue;
            yield return new WaitForEndOfFrame();
        }

        angleValue = directionMultiplier * rotationValues[rotationIndex];
        waterSprite.material.SetFloat("_ScaleAndRotationMultiplied", scaleAndRotationMultiplierCurve.Evaluate(angleValue)); 
        waterSprite.material.SetFloat("_FillAmount", fillAmountCurve.Evaluate(angleValue));
        
        numberOfWaters -= numberOfWatersToPour;
        tubeRef.numberOfWaters += numberOfWatersToPour;
        
        lr.enabled = false;
        StartCoroutine(RotateTubeBack());
    }

    protected IEnumerator RotateTubeBack()
    {
        float t = 0;
        float lerpValue;
        float angleValue;
        
        float lastAngleValue = directionMultiplier * rotationValues[rotationIndex];

        while (t < timeToRotate)
        {
            lerpValue = t / timeToRotate;
            angleValue = Mathf.Lerp(directionMultiplier * rotationValues[rotationIndex], 0, lerpValue);
            
            transform.RotateAround(choseRotationPoint.position, Vector3.forward, lastAngleValue - angleValue);
            waterSprite.material.SetFloat("_ScaleAndRotationMultiplied", scaleAndRotationMultiplierCurve.Evaluate(angleValue));

            lastAngleValue = angleValue;
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        UpdateTopWaterColor();
        angleValue = 0f;
        transform.eulerAngles = new Vector3(0, 0, angleValue);
        waterSprite.material.SetFloat("_ScaleAndRotationMultiplied", scaleAndRotationMultiplierCurve.Evaluate(angleValue)); 
        StartCoroutine(MoveTubeBack());
    }

    protected IEnumerator MoveTube()
    {
        startPos = transform.position;
        if (choseRotationPoint == leftRotationPoint)
        {
            endPos = tubeRef.rightRotationPoint.position + new Vector3(0.2f, 0, 0);
        }
        else
        {
            endPos = tubeRef.leftRotationPoint.position - new Vector3(0.2f, 0, 0);
        }
        
        float t = 0;
        while (t <= 1)
        {
            transform.position = Vector3.Lerp(startPos, endPos, t);
            t += Time.deltaTime * 2;
            
            yield return new WaitForEndOfFrame();
        }
        transform.position = endPos;
        StartCoroutine(RotateTube());
    }

    protected IEnumerator MoveTubeBack()
    {
        startPos = transform.position;
        endPos = originalPos;
        
        float t = 0;
        while (t <= 1)
        {
            transform.position = Vector3.Lerp(startPos, endPos, t);
            t += Time.deltaTime * 2;
            
            yield return new WaitForEndOfFrame();
        }
        transform.position = endPos;
    }
    
    public void UpdateTopWaterColor()
    {
        if (numberOfWaters == 0) return;
        numberOfTopColorLayers = 1;
        topColor = waterColors[numberOfWaters - 1];
        
        switch (numberOfWaters)
        {
            case 4:
            {
                if (ColorsEqual(waterColors[3], waterColors[2]))
                {
                    numberOfTopColorLayers = 2;
                    if (ColorsEqual(waterColors[2], waterColors[1]))
                    {
                        numberOfTopColorLayers = 3;
                        if (ColorsEqual(waterColors[1], waterColors[0]))
                        {
                            numberOfTopColorLayers = 4;
                        }
                    }
                }

                break;
            }
            case 3:
            {
                if (ColorsEqual(waterColors[2], waterColors[1]))
                {
                    numberOfTopColorLayers = 2;
                    if (ColorsEqual(waterColors[1], waterColors[0]))
                    {
                        numberOfTopColorLayers = 3;
                    }
                }

                break;
            }
            case 2:
            {
                if (ColorsEqual(waterColors[1], waterColors[0]))
                {
                    numberOfTopColorLayers = 2;
                }

                break;
            }
        }
        rotationIndex = 3 - (numberOfWaters - numberOfTopColorLayers);
    }

    public bool FillTubeCheck(Color colorToCheck)
    {
        if(numberOfWaters == 0) return true;
        else
        {
            if (numberOfWaters == 4) return false;
            else
            {
                return ColorsEqual(topColor, colorToCheck);
            }
        }
    }
    
    protected bool ColorsEqual(Color a, Color b, float threshold = 0.01f)
    {
        return Mathf.Abs(a.r - b.r) < threshold &&
               Mathf.Abs(a.g - b.g) < threshold &&
               Mathf.Abs(a.b - b.b) < threshold &&
               Mathf.Abs(a.a - b.a) < threshold;
    }

    protected void CalculateRotationIndex(int numberOfEmptyWatersInSecondTube)
    {
        rotationIndex = 3 - (numberOfWaters - Mathf.Min(numberOfEmptyWatersInSecondTube, numberOfTopColorLayers)); 
    }

    protected void FillUpTube(float fillAmountToAdd)
    {
        waterSprite.material.SetFloat("_FillAmount", waterSprite.material.GetFloat("_FillAmount") + fillAmountToAdd);
    }

    protected void ChooseRotationPointAndDirection()
    {
        if (transform.position.x > tubeRef.transform.position.x)
        {
            choseRotationPoint = leftRotationPoint;
            directionMultiplier = -1f;
        }
        else
        {
            choseRotationPoint = rightRotationPoint;
            directionMultiplier = 1f;
        }
    }
}