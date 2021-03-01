using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Difficulty
{
    DEFAULT,
    EASY,
    MEDIUM,
    HARD,

}

public class LockpickManager : MonoBehaviour
{
    [SerializeField]
    private Difficulty currentDifficulty;

    [SerializeField]
    private Text changeDifficultyText;

    [SerializeField]
    private Text currentDifficultyText;

    [SerializeField]
    private int lockpickingSkill = 10;

    [SerializeField]
    private int lockpickingSkillMin = 10;

    [SerializeField]
    private int lockpickingSkillMax = 30;

    [SerializeField]
    private Slider lockpickSkillSlider;

    [SerializeField]
    private TMPro.TextMeshProUGUI lockpickingSkillText;

    [SerializeField]
    private Vector3 mousePos;

    // Angle we Create by moving
    [SerializeField]
    private int currentAngle;

    // How close we are to unlock the Lock in percent
    [SerializeField]
    private float unlockPercent;

    // unlock Percent Ratio
    [SerializeField]
    private float unlockPercentRatio;

    // smooth increase of the unlockPercent
    [SerializeField]
    private float smoothUnlockPercent;

    // Random Pick Angle we have to guess
    [SerializeField]
    private int pickAngle;

    // How much around the Angle we can guess 
    [SerializeField]
    private int sweetSpotLeniency;

    // the difference for the min and max of our pick angle (the sweetspot leniency * 2)
    [SerializeField]
    private int sweetSpotDiff;

    [SerializeField]
    private int checkInt;

    // Timer to unlock the lock once we guess the right angle
    [SerializeField]
    private float unlockTimer;

    // The time we have to get to before we can unlock the lock
    [SerializeField]
    private float timeToUnlock = 1.0f;

    [SerializeField]
    private float timer = 0.0f;

    [SerializeField]
    private float lockTimer = 30.0f;

    [SerializeField]
    private TMPro.TextMeshProUGUI timerText;

    [SerializeField]
    private Coroutine timerCoroutine;

    [SerializeField]
    private bool timerRunning = false;
    [SerializeField]
    private bool checkingLock = false;
    [SerializeField]
    private bool lockUnlocked = false;

    [SerializeField]
    private GameObject pickObject;

    [SerializeField]
    private GameObject lockObject;

    private void Start()
    {
        updateLockpickingSkillText();
        ChangeDifficulty();
        findRandomAngle();
    }

    private void Update()
    {
        UpdateAngle();
        CheckAngleClick();
    }

    private void findRandomAngle()
    {
        pickAngle = Random.Range(0, 180);
    }

    private void UpdateAngle()
    {
        if(Mathf.Abs(Input.mousePosition.x - mousePos.x) > 10.0f)
        {
            int mouseXDiff = (int)(Input.mousePosition.x - mousePos.x) / 10;

            if (!checkingLock)
                currentAngle += mouseXDiff;

            if (currentAngle <= 0)
                currentAngle = 0;

            else if (currentAngle >= 180)
                currentAngle = 180;

            mousePos = Input.mousePosition;
            pickObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -currentAngle + 90.0f);
        }
    }

    // Easy Mode
    // If Current Angle is 45
    // and Guess Angle is 90
    // Move Angle for easy mode is 3 * 15 = 45
    // 90 - 45 = 45
    // 90 + 45 = 135
    // Pick Sweet spot is 15 on easy mode
    // 90 - 15 = 75
    // 90 + 15 = 105
    // Within 75 - 105 will create be able to pick the lock
    // Sweetspot difference = 15 * 2 on easy
    // CurrentAngle % Guess Angle = number between 1 and 45, we only care about 1 to 30 anything past that is bonus
    // ^ this number / sweetspot diff = a Percent of how Close we are to unlock the lock
    // fill percent will start increasing, at ~45 angle inbetween
    // Cur Angl   Fill Percent
    // 45       =       0%
    // 60       =       50%
    // 75       =       100%
    // 90       =       100%
    // 105      =       100%
    // 120      =       50%
    // 135      =       0%

    private void CheckAngleClick()
    {
        if (!lockUnlocked)
        {
            if (Input.GetKey(KeyCode.E))
            {
                if (!checkingLock)
                    checkingLock = true;

                if (currentAngle < pickAngle - sweetSpotLeniency && currentAngle >= pickAngle - sweetSpotDiff)
                    checkInt = currentAngle % (pickAngle - sweetSpotDiff);

                else if (currentAngle > pickAngle + sweetSpotLeniency && currentAngle <= pickAngle + sweetSpotDiff)
                    checkInt = (pickAngle + sweetSpotDiff) % currentAngle;

                else if (currentAngle < pickAngle - sweetSpotDiff || currentAngle > pickAngle + sweetSpotDiff)
                    checkInt = 0;

                else
                    checkInt = sweetSpotDiff - sweetSpotLeniency;

                unlockPercentRatio = (float)checkInt / (float)(sweetSpotDiff - sweetSpotLeniency);
                unlockPercent = unlockPercentRatio * 100.0f;

                unlockTimer += Time.deltaTime;

                if (unlockTimer >= (unlockPercent / 100.0f) * timeToUnlock)
                {
                    unlockTimer = (unlockPercent / 100.0f) * timeToUnlock;

                    if (unlockTimer >= timeToUnlock)
                    {
                        unlockPercent = 100;
                        Debug.Log("Unlocked");
                        increaseLockpickingSkill(5);
                        lockUnlocked = true;
                        timerRunning = false;
                        StopCoroutine(timerCoroutine);
                    }
                    else
                    {
                        Debug.Log("Failed to Unlock try another Location: " + unlockPercent + " % close.");
                    }
                }

                // * 90.0f for degrees
                smoothUnlockPercent = unlockTimer / timeToUnlock * 90.0f;
                lockObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -smoothUnlockPercent);
            }
            else
            {
                if (checkingLock)
                    checkingLock = false;

                if (unlockTimer != 0.0f)
                {
                    unlockTimer = 0.0f;
                    lockObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                }

            }
        }
    }

    public void updateLockpickingSkillText()
    {
        lockpickingSkillText.text = lockpickingSkill.ToString();
    }

    public void updateLockpickingSliderMinMax()
    {
        lockpickSkillSlider.minValue = lockpickingSkillMin;
        lockpickSkillSlider.maxValue = lockpickingSkillMax;
    }

    public void increaseLockpickingSkill(int lockpickIncrease)
    {
        lockpickingSkillMax += lockpickIncrease;

        updateLockpickingSliderMinMax();

        sweetSpotDiff -= lockpickingSkill / 2;
        sweetSpotLeniency -= lockpickingSkill / 4;

        lockpickingSkill += lockpickIncrease;
        updateLockpickingSkillText();

        if(lockpickingSkill >= lockpickingSkillMax)
        {
            lockpickingSkill = lockpickingSkillMax;
        }

        sweetSpotDiff += lockpickingSkill / 2;
        sweetSpotLeniency += lockpickingSkill / 4;
    }

    public void decreaseLockpickingSkill(int lockpickDecrease)
    {
        lockpickingSkillMin -= lockpickDecrease;

        updateLockpickingSliderMinMax();

        sweetSpotDiff -= lockpickingSkill / 2;
        sweetSpotLeniency -= lockpickingSkill / 4;
        lockpickingSkill -= lockpickDecrease;
        updateLockpickingSkillText();

        if (lockpickingSkill <= lockpickingSkillMin)
        {
            lockpickingSkill = lockpickingSkillMin;
        }

        sweetSpotDiff += lockpickingSkill / 2;
        sweetSpotLeniency += lockpickingSkill / 4;
    }

    public void sliderLockpickingSkill(Slider lockpickSlider)
    {
        sweetSpotDiff -= lockpickingSkill / 2;
        lockpickingSkill = (int)lockpickSlider.value;
        updateLockpickingSkillText();

        sweetSpotDiff += lockpickingSkill / 2;
    }

    public void startTimer()
    {
        Time.timeScale = 0.0f;
        timerRunning = true;
        timerCoroutine = StartCoroutine(startLockCountdown());
    }

    public void readyButtonPressed()
    {
        Time.timeScale = 1.0f;
        lockUnlocked = false;
    }
    public IEnumerator startLockCountdown()
    {
        timer = lockTimer;
        timerText.text = timer.ToString();

        while (timerRunning)
        {
            float dt = Time.deltaTime;
            yield return new WaitForSeconds(dt);
            timer -= dt;
            timerText.text = timer.ToString();

            if (timer <= 0.0f)
            {
                decreaseLockpickingSkill(2);
                resetLockCountdownTimer();
            }
        }
    }

    public void pauseGame()
    {
        Time.timeScale = 0.0f;
    }
    public void resetLockCountdownTimer()
    {
        lockUnlocked = true;
        timerRunning = false;
        StopCoroutine(timerCoroutine);
        timer = 0.0f;
        timerText.text = timer.ToString();
    }

    public void ChangeDifficulty()
    {
        switch(currentDifficulty)
        {
            case Difficulty.EASY:
                currentDifficulty = Difficulty.MEDIUM;
                sweetSpotLeniency = 10;
                currentDifficultyText.text = "Medium";
                changeDifficultyText.text = "Hard";
                lockTimer = 20.0f;
                break;

            case Difficulty.MEDIUM:
                currentDifficulty = Difficulty.HARD;
                sweetSpotLeniency = 5;
                currentDifficultyText.text = "Hard";
                changeDifficultyText.text = "Easy";
                lockTimer = 15.0f;
                break;

            case Difficulty.HARD:
                currentDifficulty = Difficulty.EASY;
                sweetSpotLeniency = 15;
                currentDifficultyText.text = "Easy";
                changeDifficultyText.text = "Medium";
                lockTimer = 30.0f;
                break;

            default:
                currentDifficulty = Difficulty.EASY;
                sweetSpotLeniency = 15;
                currentDifficultyText.text = "Easy";
                changeDifficultyText.text = "Medium";
                lockTimer = 30.0f;
                break;
        }
        sweetSpotDiff = (sweetSpotLeniency * 2) + lockpickingSkill / 2;
    }

}
