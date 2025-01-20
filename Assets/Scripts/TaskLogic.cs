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
    private bool waitingForSpace = false;
    private int currentBlock = 1;
    private ColorAnswer colorAnswer;
    private int trialCounter = 0;
    private int nbTrainingTrials = 0;
    private int nbMainTrials = 0;
    private bool isRestingState = false;
    private float restingTimer = 0.0f;
    private int restingCounter = 0;
    private bool isAnswerCorrect = false;
    private Trial currentTrial;
    private float generalTimer = 0.0f;
    private string participantAnswer;
    private int restingStateCounter = 0;

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
    private bool isTrainingDone = false;
    private bool doTraining = true;

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
                if (currentBlock > nbBlocks)
                {
                    //logs by block
                    for (int i = 0; i < nbBlocks; i++)
                    {
                        if(i == 0 && doTraining)
                        {
                            taskLogger.WriteToFile("Block " + (i + 1) + " - Training trials");
                            taskLogger.WriteToFile("Neutral counter: " + neutralCounterT[i] + ", Neutral correct: " + correctNeutralCounterT[i] + ", Neutral RT: " + neutralRTT[i] / neutralRTCounterT[i]);
                            taskLogger.WriteToFile("Congruent counter: " + congruentCounterT[i] + ", Congruent correct: " + correctCongruentCounterT[i] + ", Congruent RT: " + congruentRTT[i] / congruentRTCounterT[i]);
                            taskLogger.WriteToFile("Incongruent counter: " + incongruentCounterT[i] + ", Incongruent correct: " + correctIncongruentCounterT[i] + ", Incongruent RT: " + incongruentRTT[i] / incongruentRTCounterT[i]);
                        }
                        taskLogger.WriteToFile("Block " + (i + 1) + " - Main trials");
                        taskLogger.WriteToFile("Neutral counter: " + neutralCounterM[i] + ", Neutral correct: " + correctNeutralCounterM[i] + ", Neutral RT: " + neutralRTM[i] / neutralRTCounterM[i]);
                        taskLogger.WriteToFile("Congruent counter: " + congruentCounterM[i] + ", Congruent correct: " + correctCongruentCounterM[i] + ", Congruent RT: " + congruentRTM[i] / congruentRTCounterM[i]);
                        taskLogger.WriteToFile("Incongruent counter: " + incongruentCounterM[i] + ", Incongruent correct: " + correctIncongruentCounterM[i] + ", Incongruent RT: " + incongruentRTM[i] / incongruentRTCounterM[i]);
                    }
                    // total logs                 
                    taskLogger.WriteToFile("Total - Main trials");
                    taskLogger.WriteToFile("Neutral counter: " + neutralCounterM.Sum() + ", Neutral correct: " + correctNeutralCounterM.Sum() + ", Neutral RT: " + neutralRTM.Sum() / neutralRTCounterM.Sum());
                    taskLogger.WriteToFile("Congruent counter: " + congruentCounterM.Sum() + ", Congruent correct: " + correctCongruentCounterM.Sum() + ", Congruent RT: " + congruentRTM.Sum() / congruentRTCounterM.Sum());
                    taskLogger.WriteToFile("Incongruent counter: " + incongruentCounterM.Sum() + ", Incongruent correct: " + correctIncongruentCounterM.Sum() + ", Incongruent RT: " + incongruentRTM.Sum() / incongruentRTCounterM.Sum());


                    taskLogger.CloseFile();
                    Debug.Log("Task completed.");
                }
                else
                {
                    if(restingCounter % 2 == 0)
                    {
                        // We finished a block, now wait to start resting period for the next block
                        taskState = TaskState.WAITINGFORRESTINGSTART;
                    }
                    else
                    {
                        if(currentBlock == 1 && doTraining)
                        {
                            taskState = TaskState.WAITFORTRAININGSTART;
                        }
                        else
                        {
                            taskState = TaskState.WAITFORMAINSTART;
                        }
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
                    neutralCounterT[currentBlock - 1]++;
                    if (isAnswerCorrect)
                    {
                        correctNeutralCounterT[currentBlock - 1]++;
                    }
                    if(stimTimer < stimMaxDuration)
                    {
                        neutralRTT[currentBlock - 1] += stimTimer;
                        neutralRTCounterT[currentBlock - 1]++;
                    }
                    break;
                case TrialType.CONGRUENT:
                    congruentCounterT[currentBlock - 1]++;
                    if (isAnswerCorrect)
                    {
                        correctCongruentCounterT[currentBlock - 1]++;
                    }
                    if (stimTimer < stimMaxDuration)
                    {
                        congruentRTT[currentBlock - 1] += stimTimer;
                        congruentRTCounterT[currentBlock - 1]++;
                    }
                    break;
                case TrialType.INCONGRUENT:
                    incongruentCounterT[currentBlock - 1]++;
                    if (isAnswerCorrect)
                    {
                        correctIncongruentCounterT[currentBlock - 1]++;
                    }
                    if (stimTimer < stimMaxDuration)
                    {
                        incongruentRTT[currentBlock - 1] += stimTimer;
                        incongruentRTCounterT[currentBlock - 1]++;
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
                    neutralCounterM[currentBlock - 1]++;
                    if (isAnswerCorrect)
                    {
                        correctNeutralCounterM[currentBlock - 1]++;
                    }
                    if (stimTimer < stimMaxDuration)
                    {
                        neutralRTM[currentBlock - 1] += stimTimer;
                        neutralRTCounterM[currentBlock - 1]++;
                    }
                    break;
                case TrialType.CONGRUENT:
                    congruentCounterM[currentBlock - 1]++;
                    if (isAnswerCorrect)
                    {
                        correctCongruentCounterM[currentBlock - 1]++;
                    }
                    if (stimTimer < stimMaxDuration)
                    {
                        congruentRTM[currentBlock - 1] += stimTimer;
                        congruentRTCounterM[currentBlock - 1]++;
                    }
                    break;
                case TrialType.INCONGRUENT:
                    incongruentCounterM[currentBlock - 1]++;
                    if (isAnswerCorrect)
                    {
                        correctIncongruentCounterM[currentBlock - 1]++;
                    }
                    if (stimTimer < stimMaxDuration)
                    {
                        incongruentRTM[currentBlock - 1] += stimTimer;
                        incongruentRTCounterM[currentBlock - 1]++;
                    }
                    break;
            }
        }

        bool isTraining = nbMainTrials == mainTrials.Count;
        taskLogger.WriteToFile(currentBlock + ", " + isTraining + ", " + trialCounter + ", " + currentTrial.type + ", " + currentTrial.word + ", " + currentTrial.color + ", " + colorAnswer + ", " + participantAnswer + ", " + isAnswerCorrect + ", " + stimTimer + ", " + generalTimer);
        float itiDuration = Random.Range(itiMinDuration, itiMaxDuration);
        ShowBlankScreen();
        yield return new WaitForSeconds(itiDuration);
        if (trainingTrials.Count > 0 && !isTrainingDone && doTraining)
        {
            if(trainingTrials.Count == 1)
            {
                waitingForSpace = true;
                isTrainingDone = true;
            }
            showNextTrainingTrial = true;
        }
        else
        {
            if (mainTrials.Count > 0)
            {
                if(waitingForSpace)
                {
                    taskState = TaskState.WAITFORMAINSTART;
                    yield return null;
                }
                else
                {
                    showNextMainTrial = true;
                }
            }
            else
            {
                if(currentBlock < nbBlocks)
                {
                    currentBlock++;
                    showNextTrainingTrial = false;
                    showNextMainTrial = false;
                    stimIsActive = false;
                    stimTimer = 0.0f;
                    stimTriggered = false;
                    taskState = TaskState.WAITINGFORRESTINGSTART;
                    waitingForSpace = false;
                    CreateTrialsList();
                }
                else
                {
                    currentBlock++;
                    taskState = TaskState.WAITINGFORRESTINGSTART;
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
        doTraining = bool.Parse(parameters[0][15]);
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
        nbTrainingTrials = trainingTrials.Count;

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
        nbMainTrials = mainTrials.Count;

        if (mainTrials.Count == 0)
        {
            Debug.LogError("Error: No trials created.");
        }

        for(int i=0; i<nbBlocks;i++)
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
        taskLogger.StartTaskLogging();
        parameters = parametersReader.ReadParameters();
        SetParameters();
        CreateTrialsList();
        showNextTrainingTrial = false;
        showNextMainTrial = false;
        stimIsActive = false;
        stimTimer = 0.0f;
        stimTriggered = false;
        taskState = TaskState.WAITINGFORRESTINGSTART;
        waitingForSpace = false;
        currentBlock = 1;
        restingCounter = 0;
        isRestingState = false;
        restingTimer = restingTime;
        isAnswerCorrect = false;
        generalTimer = 0.0f;
        taskLogger.WriteToFile("Block, isTraining, TrialNb, TrialType, Word, Color, CorrectAnswer, ParticipantAnswer, IsCorrect, ReactionTime, timeStamp");
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
            switch (taskState)
            {
                case TaskState.WAITFORTRAININGSTART:
                    trialCounter = 0;
                    showNextTrainingTrial = true;
                    taskState = TaskState.NOTWAITING;
                    waitingForSpace = false;
                    break;
                case TaskState.WAITFORMAINSTART:
                    trialCounter = 0;
                    showNextMainTrial = true;
                    taskState = TaskState.NOTWAITING;
                    waitingForSpace = false;
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
}
