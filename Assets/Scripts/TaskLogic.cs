using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Uduino;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class TaskLogic : MonoBehaviour
{
    [SerializeField] private ParametersReader parametersReader;
    [SerializeField] private TaskLogger taskLogger;
    [SerializeField] private GameObject fixationCross;
    [SerializeField] private TextMeshProUGUI infoTxt;
    [SerializeField] private GameObject trainingButtonGO;
    [SerializeField] private GameObject mainButtonGO;
    [SerializeField] private TextMeshProUGUI wordTxt;
    [SerializeField] private KeyCode startKey = KeyCode.Space;
    [SerializeField] private KeyCode greenKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode blueKey = KeyCode.Alpha2;
    [SerializeField] private KeyCode redKey = KeyCode.Alpha3;
    [SerializeField] private KeyCode yellowKey = KeyCode.Alpha4;
    [SerializeField] private int triggerNeutralStim = 1;
    [SerializeField] private int triggerCongruentStim = 3;
    [SerializeField] private int triggerIncongruentStim = 5;
    [SerializeField] private int triggerNeutralAnswer = 11;
    [SerializeField] private int triggerCongruentAnswer = 33;
    [SerializeField] private int triggerIncongruentAnswer = 55;
    [SerializeField] private int triggerResting1 = 7;
    [SerializeField] private int triggerResting2 = 9;
    [SerializeField] private int triggerResting3 = 77;
    [SerializeField] private int triggerResting4 = 99;

    // Trial definitions and counts
    private enum TrialType
    {
        NEUTRAL,
        CONGRUENT,
        INCONGRUENT
    };

    private enum ColorAnswer
    {
        green,
        red,
        blue,
        yellow
    };

    private enum  TaskState
    {
        WAITFORTRAININGSTART,
        WAITFORMAINSTART,
        WAITINGFORRESTINGSTART,
        NOTWAITING
    }

    // Structure to hold trial information
    private struct Trial
    {
        public TrialType type;
        public string word;
        public ColorAnswer color;
        public bool isTraining;

        public Trial(TrialType type, string word, ColorAnswer color, bool isTraining)
        {
            this.type = type;
            this.word = word;
            this.color = color;
            this.isTraining = isTraining;
        }
    };

    private List<List<string>> parameters;
    private List<Trial> trainingTrials;
    private List<Trial> mainTrials;

    private bool showNextTrainingTrial = false;
    private bool showNextMainTrial = false;
    private bool stimIsActive = false;
    private float stimTimer = 0.0f;
    private bool stimTriggered = false;
    private TaskState taskState = TaskState.WAITFORTRAININGSTART;
    private int currentBlock = 1;
    private ColorAnswer colorAnswer;
    private int trialCounter = 0;
    private bool isRestingState = false;
    private float restingTimer = 0.0f;
    private int restingCounter = 0;
    private bool isAnswerCorrect = false;
    private Trial currentTrial;
    private float generalTimer = 0.0f;
    private string participantAnswer;
    private int restingStateCounter = 0;
    private int nbPhases = 2;
    private int phaseNb = 1;
    private int isPhase2 = 0;

    private float fixCrossDuration;
    private float blankDuration;
    private float stimMaxDuration;
    private float itiMinDuration;
    private float itiMaxDuration;
    private int tNeutralRep;
    private int tCongRep;
    private int tIncongRep;
    private bool tRand;
    private int mNeutralRep;
    private int mCongRep;
    private int mIncongRep;
    private bool mRand;
    private int nbBlocks;
    private float restingTime;
    private bool isTraining = false;

    // Stats variables
    
    private List<int> neutralCounterT = new List<int>();
    private List<int> congruentCounterT = new List<int>();
    private List<int> incongruentCounterT = new List<int>();
    private List<int> neutralCounterM = new List<int>();
    private List<int> congruentCounterM = new List<int>();
    private List<int> incongruentCounterM = new List<int>();
    private List<int> correctNeutralCounterT = new List<int>();
    private List<int> correctCongruentCounterT = new List<int>();
    private List<int> correctIncongruentCounterT = new List<int>();
    private List<int> correctNeutralCounterM = new List<int>();
    private List<int> correctCongruentCounterM = new List<int>();
    private List<int> correctIncongruentCounterM = new List<int>();
    private List<float> correctNeutralRTM = new List<float>();
    private List<float> correctCongruentRTM = new List<float>();
    private List<float> correctIncongruentRTM = new List<float>();
    private List<float> neutralRTT = new List<float>();
    private List<float> congruentRTT = new List<float>();
    private List<float> incongruentRTT = new List<float>();
    private List<float> neutralRTM = new List<float>();
    private List<float> congruentRTM = new List<float>();
    private List<float> incongruentRTM = new List<float>();
    private List<int> neutralRTCounterT = new List<int>();
    private List<int> congruentRTCounterT = new List<int>();
    private List<int> incongruentRTCounterT = new List<int>();
    private List<int> neutralRTCounterM = new List<int>();
    private List<int> congruentRTCounterM = new List<int>();
    private List<int> incongruentRTCounterM = new List<int>();
    
    // Start is called before the first frame update
    void Start()
    {
        Setup();
    }

    // Update is called once per frame
    void Update()
    {
        CheckKeyboardInputs();
        generalTimer += Time.deltaTime;

        if (isRestingState)
        {
            restingTimer -= Time.deltaTime;
            if (restingTimer <= 0.0f)
            {
                isRestingState = false;
                fixationCross.SetActive(false);
                if (currentBlock > nbBlocks && nbPhases == phaseNb)
                {
                    for(int i = 0; i<nbPhases; i++)
                    {
                        for(int j = 0; j<nbBlocks; j++)
                        {
                            taskLogger.WriteToFile("Phase " + (i + 1) + " - Block " + (j + 1) + " - Main trials");
                            taskLogger.WriteToFile("Neutral counter: " + neutralCounterM[j + (nbBlocks * i)] + ", Neutral correct: " + correctNeutralCounterM[j + (nbBlocks * i)] + ", Neutral RT: " + neutralRTM[j + (nbBlocks * i)] / neutralRTCounterM[j + (nbBlocks * i)] + ", Neutral correct RT: " + correctNeutralRTM[j + (nbBlocks * i)] / correctNeutralCounterM[j + (nbBlocks * i)]);
                            taskLogger.WriteToFile("Congruent counter: " + congruentCounterM[j + (nbBlocks * i)] + ", Congruent correct: " + correctCongruentCounterM[j + (nbBlocks * i)] + ", Congruent RT: " + congruentRTM[j + (nbBlocks * i)] / congruentRTCounterM[j + (nbBlocks * i)] + ", Congruent correct RT: " + correctCongruentRTM[j + (nbBlocks * i)] / correctCongruentCounterM[j + (nbBlocks * i)]);
                            taskLogger.WriteToFile("Incongruent counter: " + incongruentCounterM[j + (nbBlocks * i)] + ", Incongruent correct: " + correctIncongruentCounterM[j + (nbBlocks * i)] + ", Incongruent RT: " + incongruentRTM[j + (nbBlocks * i)] / incongruentRTCounterM[j + (nbBlocks * i)] + ", Incongruent correct RT: " + correctIncongruentRTM[j + (nbBlocks * i)] / correctIncongruentCounterM[j + (nbBlocks * i)]);
                        }
                        // total phase logs
                        taskLogger.WriteToFile("Total Phase " + (i + 1) + " - Main trials");
                        taskLogger.WriteToFile("Neutral counter: " + neutralCounterM.Skip(i * nbBlocks).Take(nbBlocks).Sum() + ", Neutral correct: " + correctNeutralCounterM.Skip(i * nbBlocks).Take(nbBlocks).Sum() + ", Neutral RT: " + neutralRTM.Skip(i * nbBlocks).Take(nbBlocks).Sum() / neutralRTCounterM.Skip(i * nbBlocks).Take(nbBlocks).Sum() + ", Neutral correct RT: " + correctNeutralRTM.Skip(i * nbBlocks).Take(nbBlocks).Sum() / correctNeutralCounterM.Skip(i * nbBlocks).Take(nbBlocks).Sum());
                        taskLogger.WriteToFile("Congruent counter: " + congruentCounterM.Skip(i * nbBlocks).Take(nbBlocks).Sum() + ", Congruent correct: " + correctCongruentCounterM.Skip(i * nbBlocks).Take(nbBlocks).Sum() + ", Congruent RT: " + congruentRTM.Skip(i * nbBlocks).Take(nbBlocks).Sum() / congruentRTCounterM.Skip(i * nbBlocks).Take(nbBlocks).Sum() + ", Congruent correct RT: " + correctCongruentRTM.Skip(i * nbBlocks).Take(nbBlocks).Sum() / correctCongruentCounterM.Skip(i * nbBlocks).Take(nbBlocks).Sum());
                        taskLogger.WriteToFile("Incongruent counter: " + incongruentCounterM.Skip(i * nbBlocks).Take(nbBlocks).Sum() + ", Incongruent correct: " + correctIncongruentCounterM.Skip(i * nbBlocks).Take(nbBlocks).Sum() + ", Incongruent RT: " + incongruentRTM.Skip(i * nbBlocks).Take(nbBlocks).Sum() / incongruentRTCounterM.Skip(i * nbBlocks).Take(nbBlocks).Sum() + ", Incongruent correct RT: " + correctIncongruentRTM.Skip(i * nbBlocks).Take(nbBlocks).Sum() / correctIncongruentCounterM.Skip(i * nbBlocks).Take(nbBlocks).Sum());
                    }
                    // total logs
                    taskLogger.WriteToFile("Total - Main trials");
                    taskLogger.WriteToFile("Neutral counter: " + neutralCounterM.Sum() + ", Neutral correct: " + correctNeutralCounterM.Sum() + ", Neutral RT: " + neutralRTM.Sum() / neutralRTCounterM.Sum() + ", Neutral correct RT: " + correctNeutralRTM.Sum() / correctNeutralCounterM.Sum());
                    taskLogger.WriteToFile("Congruent counter: " + congruentCounterM.Sum() + ", Congruent correct: " + correctCongruentCounterM.Sum() + ", Congruent RT: " + congruentRTM.Sum() / congruentRTCounterM.Sum() + ", Congruent correct RT: " + correctCongruentRTM.Sum() / correctCongruentCounterM.Sum());
                    taskLogger.WriteToFile("Incongruent counter: " + incongruentCounterM.Sum() + ", Incongruent correct: " + correctIncongruentCounterM.Sum() + ", Incongruent RT: " + incongruentRTM.Sum() / incongruentRTCounterM.Sum() + ", Incongruent correct RT: " + correctIncongruentRTM.Sum() / correctIncongruentCounterM.Sum());



                    /*
                    //logs by block
                    for (int i = 0; i < nbBlocks; i++)
                    {
                        taskLogger.WriteToFile("Block " + (i + 1) + " - Main trials");
                        taskLogger.WriteToFile("Neutral counter: " + neutralCounterM[i] + ", Neutral correct: " + correctNeutralCounterM[i] + ", Neutral RT: " + neutralRTM[i] / neutralRTCounterM[i] + ", Neutral correct RT: " + correctNeutralRTM[i] / correctNeutralCounterM[i]);
                        taskLogger.WriteToFile("Congruent counter: " + congruentCounterM[i] + ", Congruent correct: " + correctCongruentCounterM[i] + ", Congruent RT: " + congruentRTM[i] / congruentRTCounterM[i] + ", Congruent correct RT: " + correctCongruentRTM[i] / correctCongruentCounterM[i]);
                        taskLogger.WriteToFile("Incongruent counter: " + incongruentCounterM[i] + ", Incongruent correct: " + correctIncongruentCounterM[i] + ", Incongruent RT: " + incongruentRTM[i] / incongruentRTCounterM[i] + ", Incongruent correct RT: " + correctIncongruentRTM[i] / correctIncongruentCounterM[i]);
                    }
                    // total logs                 
                    taskLogger.WriteToFile("Total - Main trials");
                    taskLogger.WriteToFile("Neutral counter: " + neutralCounterM.Sum() + ", Neutral correct: " + correctNeutralCounterM.Sum() + ", Neutral RT: " + neutralRTM.Sum() / neutralRTCounterM.Sum() + ", Neutral correct RT: " + correctNeutralRTM.Sum() / correctNeutralCounterM.Sum());
                    taskLogger.WriteToFile("Congruent counter: " + congruentCounterM.Sum() + ", Congruent correct: " + correctCongruentCounterM.Sum() + ", Congruent RT: " + congruentRTM.Sum() / congruentRTCounterM.Sum() + ", Congruent correct RT: " + correctCongruentRTM.Sum() / correctCongruentCounterM.Sum());
                    taskLogger.WriteToFile("Incongruent counter: " + incongruentCounterM.Sum() + ", Incongruent correct: " + correctIncongruentCounterM.Sum() + ", Incongruent RT: " + incongruentRTM.Sum() / incongruentRTCounterM.Sum() + ", Incongruent correct RT: " + correctIncongruentRTM.Sum() / correctIncongruentCounterM.Sum());
                    */

                    taskLogger.CloseFile();
                    infoTxt.text = "Task completed.";
                    Debug.Log("Task completed.");
                }
                else
                {
                    if (currentBlock > nbBlocks)
                    {
                        phaseNb++;
                        currentBlock = 1;
                        isPhase2 = 1;
                        CreateTrialsList();
                    }
                    if (restingCounter % 2 == 0)
                    {
                        // We finished a serie of blocks, now wait to start resting period for the next serie of blocks
                        infoTxt.text = "Press Space key to start resting period.";
                        taskState = TaskState.WAITINGFORRESTINGSTART;
                    }
                    else
                    {
                        infoTxt.text = "Press Space key to start block " + currentBlock + ".";
                        taskState = TaskState.WAITFORMAINSTART;
                    }
                }
            }
        }

        // Task logic here
        if(stimIsActive)
        {
            stimTimer += Time.deltaTime;
            if(stimTimer > stimMaxDuration)
            {
                // Log trial as timeout
                isAnswerCorrect = false;
                participantAnswer = "timeout";
                stimIsActive = false;
                StartCoroutine(ITI());
            }
            if (stimTriggered && stimTimer <= stimMaxDuration)
            {
                stimTriggered = false;
                stimIsActive = false;
                StartCoroutine(ITI());
            }
        }
        if (showNextTrainingTrial)
        {
            StartCoroutine(ExecuteNextTrainingTrial());
        }
        if (showNextMainTrial)
        {
            StartCoroutine(ExecuteNextMainTrial());
        }
    }

    private IEnumerator ExecuteNextTrainingTrial()
    {
        stimTriggered = false;
        showNextTrainingTrial = false;
        Trial trial = trainingTrials[0];
        trialCounter++;
        currentTrial = trial;
        trainingTrials.RemoveAt(0);
        ShowBlankScreen();
        fixationCross.SetActive(true);
        yield return new WaitForSeconds(fixCrossDuration);
        ShowBlankScreen();
        yield return new WaitForSeconds(blankDuration);
        switch(trial.type)
        {
            case TrialType.NEUTRAL:
                UduinoManager.Instance.sendCommand("setPinsHigh", triggerNeutralStim);
                break;
            case TrialType.CONGRUENT:
                UduinoManager.Instance.sendCommand("setPinsHigh", triggerCongruentStim);
                break;
            case TrialType.INCONGRUENT:
                UduinoManager.Instance.sendCommand("setPinsHigh", triggerIncongruentStim);
                break;
        }
        wordTxt.text = trial.word;
        switch (trial.color)
        {
            case ColorAnswer.green:
                wordTxt.color = Color.green;
                colorAnswer = ColorAnswer.green;
                break;
            case ColorAnswer.red:
                wordTxt.color = Color.red;
                colorAnswer = ColorAnswer.red;
                break;
            case ColorAnswer.blue:
                wordTxt.color = Color.blue;
                colorAnswer = ColorAnswer.blue;
                break;
            case ColorAnswer.yellow:
                wordTxt.color = Color.yellow;
                colorAnswer = ColorAnswer.yellow;
                break;
        }
        wordTxt.gameObject.SetActive(true);
        stimTimer = 0.0f;
        stimIsActive = true;
        yield return null;
    }

    private IEnumerator ExecuteNextMainTrial()
    {
        stimTriggered = false;
        showNextMainTrial = false;
        Trial trial = mainTrials[0];
        currentTrial = trial;
        trialCounter++;
        mainTrials.RemoveAt(0);
        ShowBlankScreen();
        fixationCross.SetActive(true);
        yield return new WaitForSeconds(fixCrossDuration);
        ShowBlankScreen();
        yield return new WaitForSeconds(blankDuration);
        switch (trial.type)
        {
            case TrialType.NEUTRAL:
                UduinoManager.Instance.sendCommand("setPinsHigh", triggerNeutralStim);
                break;
            case TrialType.CONGRUENT:
                UduinoManager.Instance.sendCommand("setPinsHigh", triggerCongruentStim);
                break;
            case TrialType.INCONGRUENT:
                UduinoManager.Instance.sendCommand("setPinsHigh", triggerIncongruentStim);
                break;
        }
        wordTxt.text = trial.word;
        switch (trial.color)
        {
            case ColorAnswer.green:
                wordTxt.color = Color.green;
                colorAnswer = ColorAnswer.green;
                break;
            case ColorAnswer.red:
                wordTxt.color = Color.red;
                colorAnswer = ColorAnswer.red;
                break;
            case ColorAnswer.blue:
                wordTxt.color = Color.blue;
                colorAnswer = ColorAnswer.blue;
                break;
            case ColorAnswer.yellow:
                wordTxt.color = Color.yellow;
                colorAnswer = ColorAnswer.yellow;
                break;
        }
        wordTxt.gameObject.SetActive(true);
        stimTimer = 0.0f;
        stimIsActive = true;
        yield return null;
    }

    private IEnumerator ITI()
    {
        if(currentTrial.isTraining)
        {
            // Training trial
            switch (currentTrial.type)
            {
                case TrialType.NEUTRAL:
                    neutralCounterT[currentBlock - 1 + (nbBlocks*isPhase2)]++;
                    if (isAnswerCorrect)
                    {
                        correctNeutralCounterT[currentBlock - 1 + (nbBlocks * isPhase2)]++;
                    }
                    if(stimTimer < stimMaxDuration)
                    {
                        neutralRTT[currentBlock - 1 + (nbBlocks * isPhase2)] += stimTimer;
                        neutralRTCounterT[currentBlock - 1 + (nbBlocks * isPhase2)]++;
                    }
                    break;
                case TrialType.CONGRUENT:
                    congruentCounterT[currentBlock - 1 + (nbBlocks * isPhase2)]++;
                    if (isAnswerCorrect)
                    {
                        correctCongruentCounterT[currentBlock - 1 + (nbBlocks * isPhase2)]++;
                    }
                    if (stimTimer < stimMaxDuration)
                    {
                        congruentRTT[currentBlock - 1 + (nbBlocks * isPhase2)] += stimTimer;
                        congruentRTCounterT[currentBlock - 1 + (nbBlocks * isPhase2)]++;
                    }
                    break;
                case TrialType.INCONGRUENT:
                    incongruentCounterT[currentBlock - 1 + (nbBlocks * isPhase2)]++;
                    if (isAnswerCorrect)
                    {
                        correctIncongruentCounterT[currentBlock - 1 + (nbBlocks * isPhase2)]++;
                    }
                    if (stimTimer < stimMaxDuration)
                    {
                        incongruentRTT[currentBlock - 1 + (nbBlocks * isPhase2)] += stimTimer;
                        incongruentRTCounterT[currentBlock - 1 + (nbBlocks * isPhase2)]++;
                    }
                    break;
            }
        }
        else
        {
            // Main trial
            switch (currentTrial.type)
            {
                case TrialType.NEUTRAL:
                    neutralCounterM[currentBlock - 1 + (nbBlocks * isPhase2)]++;
                    if (isAnswerCorrect)
                    {
                        correctNeutralCounterM[currentBlock - 1 + (nbBlocks * isPhase2)]++;
                        correctNeutralRTM[currentBlock - 1 + (nbBlocks * isPhase2)] += stimTimer;
                    }
                    if (stimTimer < stimMaxDuration || isAnswerCorrect)
                    {
                        neutralRTM[currentBlock - 1 + (nbBlocks * isPhase2)] += stimTimer;
                        neutralRTCounterM[currentBlock - 1 + (nbBlocks * isPhase2)]++;
                    }
                    break;
                case TrialType.CONGRUENT:
                    congruentCounterM[currentBlock - 1 + (nbBlocks * isPhase2)]++;
                    if (isAnswerCorrect)
                    {
                        correctCongruentCounterM[currentBlock - 1 + (nbBlocks * isPhase2)]++;
                        correctCongruentRTM[currentBlock - 1 + (nbBlocks * isPhase2)] += stimTimer;
                    }
                    if (stimTimer < stimMaxDuration || isAnswerCorrect)
                    {
                        congruentRTM[currentBlock - 1 + (nbBlocks * isPhase2)] += stimTimer;
                        congruentRTCounterM[currentBlock - 1 + (nbBlocks * isPhase2)]++;
                    }
                    break;
                case TrialType.INCONGRUENT:
                    incongruentCounterM[currentBlock - 1 + (nbBlocks * isPhase2)]++;
                    if (isAnswerCorrect)
                    {
                        correctIncongruentCounterM[currentBlock - 1 + (nbBlocks * isPhase2)]++;
                        correctIncongruentRTM[currentBlock - 1 + (nbBlocks * isPhase2)] += stimTimer;
                    }
                    if (stimTimer < stimMaxDuration || isAnswerCorrect)
                    {
                        incongruentRTM[currentBlock - 1 + (nbBlocks * isPhase2)] += stimTimer;
                        incongruentRTCounterM[currentBlock - 1 + (nbBlocks * isPhase2)]++;
                    }
                    break;
            }
        }
        if (!isTraining)
        {
            taskLogger.WriteToFile(currentBlock + ", " + trialCounter + ", " + currentTrial.type + ", " + currentTrial.word + ", " + currentTrial.color + ", " + colorAnswer + ", " + participantAnswer + ", " + isAnswerCorrect + ", " + stimTimer + ", " + generalTimer);
        }
        float itiDuration = Random.Range(itiMinDuration, itiMaxDuration);
        ShowBlankScreen();
        yield return new WaitForSeconds(itiDuration);
        if (trainingTrials.Count > 0 && isTraining)
        {
            showNextTrainingTrial = true;
        }
        else
        {
            if (isTraining)
            {
                Setup();
            }
            else
            {
                if (mainTrials.Count > 0)
                {
                    showNextMainTrial = true;
                }
                else
                {
                    if (currentBlock < nbBlocks)
                    {
                        infoTxt.text = "Block " + currentBlock + " completed. Press Space key to start the next block.";
                        currentBlock++;
                        showNextTrainingTrial = false;
                        showNextMainTrial = false;
                        stimIsActive = false;
                        stimTimer = 0.0f;
                        stimTriggered = false;
                        taskState = TaskState.WAITFORMAINSTART;
                        CreateTrialsList();
                    }
                    else
                    {
                        currentBlock++;
                        infoTxt.text = "Press Space key to start resting period.";
                        taskState = TaskState.WAITINGFORRESTINGSTART;
                    }
                }
            }   
        }
        yield return null;
    }

    private void SetParameters()
    {
        fixCrossDuration = float.Parse(parameters[0][0]);
        blankDuration = float.Parse(parameters[0][1]);
        stimMaxDuration = float.Parse(parameters[0][2]);
        itiMinDuration = float.Parse(parameters[0][3]);
        itiMaxDuration = float.Parse(parameters[0][4]);
        tNeutralRep = int.Parse(parameters[0][5]);
        tCongRep = int.Parse(parameters[0][6]);
        tIncongRep = int.Parse(parameters[0][7]);
        tRand = bool.Parse(parameters[0][8]);
        mNeutralRep = int.Parse(parameters[0][9]);
        mCongRep = int.Parse(parameters[0][10]);
        mIncongRep = int.Parse(parameters[0][11]);
        mRand = bool.Parse(parameters[0][12]);
        nbBlocks = int.Parse(parameters[0][13]);
        restingTime = int.Parse(parameters[0][14]);
    }

    private void CreateTrialsList()
    {
        trainingTrials = new List<Trial>();
        mainTrials = new List<Trial>();

        // Training trials
        for (int i = 0; i < tNeutralRep; i++)
        {
            trainingTrials.Add(new Trial(TrialType.NEUTRAL, "XXXXX", ColorAnswer.green, true));
            trainingTrials.Add(new Trial(TrialType.NEUTRAL, "XXX", ColorAnswer.red, true));
            trainingTrials.Add(new Trial(TrialType.NEUTRAL, "XXXX", ColorAnswer.blue, true));
            trainingTrials.Add(new Trial(TrialType.NEUTRAL, "XXXXXX", ColorAnswer.yellow, true));
        }
        for (int i = 0; i < tCongRep; i++)
        {
            trainingTrials.Add(new Trial(TrialType.CONGRUENT, "RED", ColorAnswer.red, true));
            trainingTrials.Add(new Trial(TrialType.CONGRUENT, "GREEN", ColorAnswer.green, true));
            trainingTrials.Add(new Trial(TrialType.CONGRUENT, "BLUE", ColorAnswer.blue, true));
            trainingTrials.Add(new Trial(TrialType.CONGRUENT, "YELLOW", ColorAnswer.yellow, true));
        }
        for (int i = 0; i < tIncongRep; i++)
        {
            trainingTrials.Add(new Trial(TrialType.INCONGRUENT, "GREEN", ColorAnswer.blue, true));
            trainingTrials.Add(new Trial(TrialType.INCONGRUENT, "GREEN", ColorAnswer.red, true));
            trainingTrials.Add(new Trial(TrialType.INCONGRUENT, "GREEN", ColorAnswer.yellow, true));
            trainingTrials.Add(new Trial(TrialType.INCONGRUENT, "BLUE", ColorAnswer.green, true));
            trainingTrials.Add(new Trial(TrialType.INCONGRUENT, "BLUE", ColorAnswer.red, true));
            trainingTrials.Add(new Trial(TrialType.INCONGRUENT, "BLUE", ColorAnswer.yellow, true));
            trainingTrials.Add(new Trial(TrialType.INCONGRUENT, "RED", ColorAnswer.green, true));
            trainingTrials.Add(new Trial(TrialType.INCONGRUENT, "RED", ColorAnswer.blue, true));
            trainingTrials.Add(new Trial(TrialType.INCONGRUENT, "RED", ColorAnswer.yellow, true));
            trainingTrials.Add(new Trial(TrialType.INCONGRUENT, "YELLOW", ColorAnswer.green, true));
            trainingTrials.Add(new Trial(TrialType.INCONGRUENT, "YELLOW", ColorAnswer.blue, true));
            trainingTrials.Add(new Trial(TrialType.INCONGRUENT, "YELLOW", ColorAnswer.red, true));
        }
        if(tRand)
        {
            trainingTrials = RandomizeListWithoutMoreThanTwoRep(trainingTrials);
        }

        // Main trials
        for (int i = 0; i < mNeutralRep; i++)
        {
            mainTrials.Add(new Trial(TrialType.NEUTRAL, "XXXXX", ColorAnswer.green, false));
            mainTrials.Add(new Trial(TrialType.NEUTRAL, "XXX", ColorAnswer.red, false));
            mainTrials.Add(new Trial(TrialType.NEUTRAL, "XXXX", ColorAnswer.blue, false));
            mainTrials.Add(new Trial(TrialType.NEUTRAL, "XXXXXX", ColorAnswer.yellow, false));
        }
        for (int i = 0; i < mCongRep; i++)
        {
            mainTrials.Add(new Trial(TrialType.CONGRUENT, "RED", ColorAnswer.red, false));
            mainTrials.Add(new Trial(TrialType.CONGRUENT, "GREEN", ColorAnswer.green, false));
            mainTrials.Add(new Trial(TrialType.CONGRUENT, "BLUE", ColorAnswer.blue, false));
            mainTrials.Add(new Trial(TrialType.CONGRUENT, "YELLOW", ColorAnswer.yellow, false));
        }
        for (int i = 0; i < mIncongRep; i++)
        {
            mainTrials.Add(new Trial(TrialType.INCONGRUENT, "GREEN", ColorAnswer.blue, false));
            mainTrials.Add(new Trial(TrialType.INCONGRUENT, "GREEN", ColorAnswer.red, false));
            mainTrials.Add(new Trial(TrialType.INCONGRUENT, "GREEN", ColorAnswer.yellow, false));
            mainTrials.Add(new Trial(TrialType.INCONGRUENT, "BLUE", ColorAnswer.green, false));
            mainTrials.Add(new Trial(TrialType.INCONGRUENT, "BLUE", ColorAnswer.red, false));
            mainTrials.Add(new Trial(TrialType.INCONGRUENT, "BLUE", ColorAnswer.yellow, false));
            mainTrials.Add(new Trial(TrialType.INCONGRUENT, "RED", ColorAnswer.green, false));
            mainTrials.Add(new Trial(TrialType.INCONGRUENT, "RED", ColorAnswer.blue, false));
            mainTrials.Add(new Trial(TrialType.INCONGRUENT, "RED", ColorAnswer.yellow, false));
            mainTrials.Add(new Trial(TrialType.INCONGRUENT, "YELLOW", ColorAnswer.green, false));
            mainTrials.Add(new Trial(TrialType.INCONGRUENT, "YELLOW", ColorAnswer.blue, false));
            mainTrials.Add(new Trial(TrialType.INCONGRUENT, "YELLOW", ColorAnswer.red, false));
        }
        if (mRand)
        {
            mainTrials = RandomizeListWithoutMoreThanTwoRep(mainTrials);
        }

        if (mainTrials.Count == 0)
        {
            Debug.LogError("Error: No trials created.");
        }

        for (int i=0; i<(nbBlocks * nbPhases);i++)
        {
            neutralCounterT.Add(0);
            congruentCounterT.Add(0);
            incongruentCounterT.Add(0);
            neutralCounterM.Add(0);
            congruentCounterM.Add(0);
            incongruentCounterM.Add(0);
            correctNeutralCounterT.Add(0);
            correctCongruentCounterT.Add(0);
            correctIncongruentCounterT.Add(0);
            correctNeutralCounterM.Add(0);
            correctCongruentCounterM.Add(0);
            correctIncongruentCounterM.Add(0);
            neutralRTT.Add(0.0f);
            congruentRTT.Add(0.0f);
            incongruentRTT.Add(0.0f);
            neutralRTM.Add(0.0f);
            congruentRTM.Add(0.0f);
            incongruentRTM.Add(0.0f);
            neutralRTCounterT.Add(0);
            congruentRTCounterT.Add(0);
            incongruentRTCounterT.Add(0);
            neutralRTCounterM.Add(0);
            congruentRTCounterM.Add(0);
            incongruentRTCounterM.Add(0);
            correctNeutralRTM.Add(0.0f);
            correctCongruentRTM.Add(0.0f);
            correctIncongruentRTM.Add(0.0f);
        }
    }

    private List<Trial> RandomizeListWithoutMoreThanTwoRep(List<Trial> trials)
    {
        // Randomize list in such a way that the same word is not repeated more than twice in a row
        List<Trial> randomizedList = new List<Trial>();
        List<Trial> tempTrials = new List<Trial>(trials);
        tempTrials = RandomizeList(tempTrials);
        for(int i = 2; i < tempTrials.Count; i++)
        {
            if (tempTrials[i].word == tempTrials[i-1].word && tempTrials[i].word == tempTrials[i-2].word)
            {
                // Find a word later in the list that is different
                int swapIndex = i + 1;
                while (swapIndex < tempTrials.Count && tempTrials[swapIndex].word == tempTrials[i].word)
                {
                    swapIndex++;
                }

                // If a valid replacement is found, swap the words
                if (swapIndex < tempTrials.Count)
                {
                    var temp = tempTrials[i];
                    tempTrials[i] = tempTrials[swapIndex];
                    tempTrials[swapIndex] = temp;
                }
            }
        }
        randomizedList = tempTrials;
        return randomizedList;
    }

    private List<Trial> RandomizeList(List<Trial> trials)
    {
        List<Trial> randomizedList = new List<Trial>();
        List<Trial> tempTrials = new List<Trial>(trials);
        int randomIndex = Random.Range(0, tempTrials.Count);
        randomizedList.Add(tempTrials[randomIndex]);
        tempTrials.RemoveAt(randomIndex);
        while (tempTrials.Count > 0)
        {
            randomIndex = Random.Range(0, tempTrials.Count);
            randomizedList.Add(tempTrials[randomIndex]);
            tempTrials.RemoveAt(randomIndex);
        }
        return randomizedList;
    }

    private void Setup()
    {
        phaseNb = 1;
        trainingButtonGO.SetActive(true);
        mainButtonGO.SetActive(true);
        parameters = parametersReader.ReadParameters();
        SetParameters();
        CreateTrialsList();
        showNextTrainingTrial = false;
        showNextMainTrial = false;
        stimIsActive = false;
        stimTimer = 0.0f;
        stimTriggered = false;
        taskState = TaskState.WAITINGFORRESTINGSTART;
        currentBlock = 1;
        restingCounter = 0;
        isRestingState = false;
        restingTimer = restingTime;
        isAnswerCorrect = false;
        generalTimer = 0.0f;
        infoTxt.text = "Select \"Training\" to start a training session or \"Main\" to start the main task.";
    }

    private void ShowBlankScreen()
    {
        fixationCross.SetActive(false);
        wordTxt.gameObject.SetActive(false);
    }

    private void CheckKeyboardInputs()
    {
        if (Input.GetKeyDown(startKey) && taskState != TaskState.NOTWAITING)
        {
            infoTxt.text = "";
            switch (taskState)
            {
                case TaskState.WAITFORTRAININGSTART:
                    trialCounter = 0;
                    showNextTrainingTrial = true;
                    taskState = TaskState.NOTWAITING;
                    break;
                case TaskState.WAITFORMAINSTART:
                    trialCounter = 0;
                    showNextMainTrial = true;
                    taskState = TaskState.NOTWAITING;
                    break;
                case TaskState.WAITINGFORRESTINGSTART:
                    
                    switch (restingStateCounter)
                    {
                        case 0:
                            UduinoManager.Instance.sendCommand("setPinsHigh", triggerResting1);
                            break;
                        case 1:
                            UduinoManager.Instance.sendCommand("setPinsHigh",triggerResting2);
                            break;
                        case 2:
                            UduinoManager.Instance.sendCommand("setPinsHigh", triggerResting3);
                            break;
                        case 3:
                            UduinoManager.Instance.sendCommand("setPinsHigh", triggerResting4);
                            break;
                    }
                    restingStateCounter++;
                    isRestingState = true;
                    restingTimer = restingTime;
                    restingCounter++;
                    fixationCross.SetActive(true);
                    taskState = TaskState.NOTWAITING;
                    break;
            }
        }

        if (Input.GetKeyDown(greenKey) && stimIsActive && !stimTriggered)
        {
            switch (currentTrial.type)
            {
                case TrialType.NEUTRAL:
                    UduinoManager.Instance.sendCommand("setPinsHigh", triggerNeutralAnswer);
                    break;
                case TrialType.CONGRUENT:
                    UduinoManager.Instance.sendCommand("setPinsHigh", triggerCongruentAnswer);
                    break;
                case TrialType.INCONGRUENT:
                    UduinoManager.Instance.sendCommand("setPinsHigh", triggerIncongruentAnswer);
                    break;
            }
            participantAnswer = "green";
            stimTriggered = true;
            if (colorAnswer == ColorAnswer.green && stimIsActive)
            {
                isAnswerCorrect = true;
            }
            else
            {
                isAnswerCorrect = false;
            }
        }
        if (Input.GetKeyDown(redKey) && stimIsActive && !stimTriggered)
        {
            switch (currentTrial.type)
            {
                case TrialType.NEUTRAL:
                    UduinoManager.Instance.sendCommand("setPinsHigh", triggerNeutralAnswer);
                    break;
                case TrialType.CONGRUENT:
                    UduinoManager.Instance.sendCommand("setPinsHigh", triggerCongruentAnswer);
                    break;
                case TrialType.INCONGRUENT:
                    UduinoManager.Instance.sendCommand("setPinsHigh", triggerIncongruentAnswer);
                    break;
            }
            participantAnswer = "red";
            stimTriggered = true;
            if (colorAnswer == ColorAnswer.red && stimIsActive)
            {
                isAnswerCorrect = true;
            }
            else
            {
                isAnswerCorrect = false;
            }
        }
        if (Input.GetKeyDown(blueKey) && stimIsActive && !stimTriggered)
        {
            switch (currentTrial.type)
            {
                case TrialType.NEUTRAL:
                    UduinoManager.Instance.sendCommand("setPinsHigh", triggerNeutralAnswer);
                    break;
                case TrialType.CONGRUENT:
                    UduinoManager.Instance.sendCommand("setPinsHigh", triggerCongruentAnswer);
                    break;
                case TrialType.INCONGRUENT:
                    UduinoManager.Instance.sendCommand("setPinsHigh", triggerIncongruentAnswer);
                    break;
            }
            participantAnswer = "blue";
            stimTriggered = true;
            if (colorAnswer == ColorAnswer.blue)
            {
                isAnswerCorrect = true;
            }
            else
            {
                isAnswerCorrect = false;
            }
        }
        if (Input.GetKeyDown(yellowKey) && stimIsActive && !stimTriggered)
        {
            switch (currentTrial.type)
            {
                case TrialType.NEUTRAL:
                    UduinoManager.Instance.sendCommand("setPinsHigh", triggerNeutralAnswer);
                    break;
                case TrialType.CONGRUENT:
                    UduinoManager.Instance.sendCommand("setPinsHigh", triggerCongruentAnswer);
                    break;
                case TrialType.INCONGRUENT:
                    UduinoManager.Instance.sendCommand("setPinsHigh", triggerIncongruentAnswer);
                    break;
            }
            participantAnswer = "yellow";
            stimTriggered = true;
            if (colorAnswer == ColorAnswer.yellow)
            {
                isAnswerCorrect = true;
            }
            else
            {
                isAnswerCorrect = false;
            }
        }
    }

    public void PressTrainingButton()
    {
        trainingButtonGO.SetActive(false);
        mainButtonGO.SetActive(false);
        isTraining = true;
        taskState = TaskState.WAITFORTRAININGSTART;
        infoTxt.text = "Press Space key to start training session.";
    }

    public void PressMainButton()
    {
        taskLogger.StartTaskLogging();
        taskLogger.WriteToFile("Block, TrialNb, TrialType, Word, Color, CorrectAnswer, ParticipantAnswer, IsCorrect, ReactionTime, timeStamp");
        trainingButtonGO.SetActive(false);
        mainButtonGO.SetActive(false);
        isTraining = false;
        taskState = TaskState.WAITINGFORRESTINGSTART;
        infoTxt.text = "Press Space key to start resting period.";
    }
}
