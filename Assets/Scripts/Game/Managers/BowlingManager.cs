using Photon.Pun;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// Responsible for managing all the aspects during a bowling game, in solo or 2 players.
/// </summary>
public class BowlingManager : MonoBehaviourPun, IPunObservable
{
    /// <summary>
    /// Gets or sets the pins with their position and rotation to reset them.
    /// </summary>
    public Dictionary<GameObject, (Vector3, Quaternion)> Pins { get; set; }

    /// <summary>
    /// Gets or sets the score of the current frame.
    /// </summary>
    public int ScoreFrame { get; set; }

    /// <summary>
    /// Gets or sets if the ball was destroyed or not.
    /// </summary>
    public bool BallDestroyed { get; set; }

    /// <summary>
    /// Gets or sets the component managing the fade in / fade out system.
    /// </summary>
    public FadeScreenManager FadeScreen { get; set; }

    /// <summary>
    /// Gets or sets the component managing the pause menu and behaviour.
    /// </summary>
    public PauseMenuManager PauseManager { get; set; }

    /// <summary>
    /// Gets or sets the GameObject displayed at the end of the game.
    /// </summary>
    public GameObject ReplayMenu { get; set; }

    public string OpponentID { get; set; }

    private Transform _ballSpawnPoint;
    private Transform _pinsSpawnPoint;
    private int[] _frameScores;
    private int _frame;
    private int _round = 1;
    private int _throw = 1;
    private int _remainingPins = 10;
    private int _score;
    private int _otherPlayerScore;
    private bool _isPlayerOne;
    private bool _canThrowLast;
    private bool _pinsChecked;
    private bool _playerOneFinished;
    private bool _playerTwoFinished;
    private ScoreWriter _scoreBoard;
    private GameObject _pins;

    /// <summary>
    /// Initializes the game fields and sets up initial configurations when the game starts.
    /// </summary>
    void Start()
    {
        _frameScores = new int[21];
        Pins = new();
        _isPlayerOne = PhotonNetwork.LocalPlayer.ActorNumber == 1;

        GameObject scoreBoard = _isPlayerOne ? GameObject.Find("Screen Player 1") : GameObject.Find("Screen Player 2");
        _scoreBoard = scoreBoard.GetComponent<ScoreWriter>();
        scoreBoard.GetPhotonView().TransferOwnership(GameManager.Instance.Player.GetPhotonView().Owner);
        _ballSpawnPoint = _isPlayerOne ? GameObject.Find("Ball Spawn Player 1").transform : GameObject.Find("Ball Spawn Player 2").transform;
        _pinsSpawnPoint = _isPlayerOne ? GameObject.Find("Pins Spawn Player 1").transform : GameObject.Find("Pins Spawn Player 2").transform;
        _pins = _isPlayerOne ? GameObject.FindWithTag("Pins1") : GameObject.FindWithTag("Pins2");

        _pins.GetPhotonView().TransferOwnership(GameManager.Instance.Player.GetPhotonView().OwnerActorNr);
        for (int i = 0; i < _pins.transform.childCount; i++)
        {
            Transform pin = _pins.transform.GetChild(i);
            Pins.Add(pin.gameObject, (pin.position, pin.rotation));
            pin.GetComponent<Pin>().enabled = true;
        }
        if (PhotonNetwork.OfflineMode || !_isPlayerOne)
            SetBall();
    }

    /// <summary>
    /// Called every frame. Checks if the bowling ball was destroyed or not.
    /// </summary>
    private void Update()
    {
        if (BallDestroyed)
            StartCoroutine(ManageRound());
        if (!PhotonNetwork.OfflineMode && _playerOneFinished && _playerTwoFinished)
            DisplayReplayMenuForBoth(GameObject.FindWithTag("Player").GetComponent<InteractorManager>(), GameManager.Instance.DataManager);
    }

    /// <summary>
    /// Manages the progression of rounds during a bowling game.
    /// </summary>
    /// <returns>Several WaitForSeconds for coroutine execution.</returns>
    private IEnumerator ManageRound()
    {
        BallDestroyed = false;
        yield return new WaitForSeconds(2f);

/*		CheckPins();
        while (!_pinsChecked)
            yield return null;*/

        PauseManager.enabled = false;
        FadeScreen.FadeOut();
        yield return new WaitForSeconds(FadeScreen.FadeDuration);

        _scoreBoard.SetCurrentSlot(_round);

        _frameScores[_frame++] = ScoreFrame;
        _remainingPins -= ScoreFrame;

        CalculateScore();
        _scoreBoard.ManageStrikeAndSpare(_throw, _score, _frame, ScoreFrame, _remainingPins, _round, _frameScores);

        if (_round == 10 && _throw == 2 && _remainingPins == 0)
            _canThrowLast = true;

        _scoreBoard.WriteScore(_score.ToString(), _round, _throw, ScoreFrame, _remainingPins);
        _scoreBoard.UpdateScoreSlots(_round);

        if (_throw == 1)
        {
            _throw++;
            if (_remainingPins == 0)
                ManagePins();
            else
                ResetPins();
        }
        else
            ManagePins();

        //UpdateData(_playerOneFinished, _playerTwoFinished, _score);
        ScoreFrame = 0;
        _pinsChecked = false;
        FadeScreen.FadeIn();
        yield return new WaitForSeconds(FadeScreen.FadeDuration);
        PauseManager.enabled = true;
    }

    /// <summary>
    /// Checks which pins are knocked over or not.
    /// </summary>
    private void CheckPins()
    {
        Transform pinsTransform = _pins.transform;
        for (int i = 0; i < pinsTransform.childCount; i++)
        {
            Transform child = pinsTransform.GetChild(i);
            GameObject pin = child.gameObject;
            if (Mathf.Abs(child.position.y - Pins[pin].Item1.y) > 0.1f && pin.GetPhotonView().IsMine)
            {
                ScoreFrame++;
                Pins.Remove(pin);
                PhotonNetwork.Destroy(pin);
            }
        }
        _pinsChecked = true;
    }

    /// <summary>
    /// Calculates the current score of the game.
    /// </summary>
    private void CalculateScore()
    {
        _score = 0;
        int frameIndex = 0;

        for (int i = 0; i < 10; i++)
        {
            if (_frameScores[frameIndex] + _frameScores[frameIndex + 1] == 10)
            {
                /* Add the score of the throw made after the spare to it */
                _score += 10 + _frameScores[frameIndex + 2];
                frameIndex += 2;
            }
            else if (_frameScores[frameIndex] == 10)
            {
                /* Add the score of the 2 throws made after the strike to it */
                _score += 10 + _frameScores[frameIndex + 1] + _frameScores[frameIndex + 2];
                frameIndex++;
            }
            else
            {
                /* Add the 2 frames of the current round */
                _score += _frameScores[frameIndex] + _frameScores[frameIndex + 1];
                frameIndex += 2;
            }
        }
    }

    /// <summary>
    /// Instantiates a new bowling ball.
    /// </summary>
    public void SetBall() => PhotonNetwork.Instantiate("BowlingBall", _ballSpawnPoint.position, _ballSpawnPoint.rotation);

    /// <summary>
    /// Instantiates a new set of pins.
    /// </summary>
    private void SetPins()
    {
        _pins = PhotonNetwork.Instantiate(_isPlayerOne ? "PinsPlayer1" : "PinsPlayer2", _pinsSpawnPoint.position, _pinsSpawnPoint.rotation);
        _pins.GetPhotonView().TransferOwnership(GameManager.Instance.Player.GetPhotonView().OwnerActorNr);

        for (int i = 0; i < _pins.transform.childCount; i++)
        {
            Transform pin = _pins.transform.GetChild(i);
            Pins.Add(pin.gameObject, (pin.position, pin.rotation));
            pin.GetComponent<Pin>().enabled = true;
        }
    }

    /// <summary>
    /// Resets the position of the pins that weren't knocked down and instantiates a new ball.
    /// </summary>
    private void ResetPins()
    {
        foreach (KeyValuePair<GameObject, (Vector3, Quaternion)> kvp in Pins)
            kvp.Key.transform.SetPositionAndRotation(kvp.Value.Item1, kvp.Value.Item2);
        SetBall();
    }


    /// <summary>
    /// Destroys the remaining pins and instantiates a new set and a ball.
    /// </summary>
    private void ManagePins()
    {
        if ((_round == 10 && _throw == 2 && !_canThrowLast) || _throw == 3)
            FinishGame();
        else
        {
            if (_pins.GetPhotonView().IsMine)
            {
                foreach (KeyValuePair<GameObject, (Vector3, Quaternion)> kvp in Pins)
                {
                    if (kvp.Key.GetPhotonView().IsMine)
                        PhotonNetwork.Destroy(kvp.Key);
                }
                PhotonNetwork.Destroy(_pins);
                _pins = null;
            }
            Pins.Clear();
            _remainingPins = 10;
            SetPins();
            SetBall();
        }
        if (_round != 10)
        {
            _round++;
            _throw--;
            _scoreBoard.ResetCurrentSlot();
        }
        else if (_round == 10 && _throw == 2 && _canThrowLast)
            _throw++;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_playerOneFinished);
            stream.SendNext(_playerTwoFinished);
            stream.SendNext(_score);
        }
        else
        {
            _playerOneFinished = (bool)stream.ReceiveNext();
            _playerTwoFinished = (bool)stream.ReceiveNext();
            _otherPlayerScore = (int)stream.ReceiveNext();
        }
    }

/*	private void UpdateData(bool playerOneFinished, bool playerTwoFinished, int score) => photonView.RpcSecure("SendData", RpcTarget.All, true, playerOneFinished, playerTwoFinished, score);

    [PunRPC]
    private void SendData(bool playerOneFinished, bool playerTwoFinished, int score)
    {
        _playerOneFinished = playerOneFinished;
        _playerTwoFinished = playerTwoFinished;
        _otherPlayerScore = score;
    }


    [PunRPC]
    private void ReceiveData(bool playerOneFinished, bool playerTwoFinished, int score)
    {
        _playerOneFinished = playerOneFinished;
        _playerTwoFinished = playerTwoFinished;
        _otherPlayerScore = score;
    }*/


    /// <summary>
    /// Puts an end to the game. If in 2-players mode, waits for both player to end their game. Displays the choice to restart a new game
    /// or return to the main menu.
    /// </summary>
    private void FinishGame()
    {
/*        DataManager dataManager = GameManager.Instance.DataManager;
        if (_score > dataManager.HighScores["Bowling"])
        {
            dataManager.HighScores["Bowling"] = _score;
            dataManager.SaveData();
        }*/

        if (PhotonNetwork.OfflineMode)
        {
            ReplayMenu.SetActive(true);
            GameObject.FindWithTag("Player").GetComponent<InteractorManager>().ToggleBehavior();
        }
        else
        {
            if (_isPlayerOne)
                _playerOneFinished = true;
            else
                _playerTwoFinished = true;
            ReplayMenu.SetActive(true);
            GameObject.FindWithTag("Player").GetComponent<InteractorManager>().ToggleBehavior();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="interactor"></param>
    /// <param name="dataManager"></param>
    private void DisplayReplayMenuForBoth(InteractorManager interactor, DataManager dataManager)
    {
/*        (int, int) result = dataManager.MatchHistory[OpponentID];
        if (_score > _otherPlayerScore)
            result.Item1++;
        else
            result.Item2++;
        dataManager.SaveData();
        ReplayMenu.SetActive(true);
        interactor.ToggleBehavior();*/
    }
}