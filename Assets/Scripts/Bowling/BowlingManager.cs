using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Collections;
using TMPro;

public class BowlingManager : MonoBehaviour
{
	public Dictionary<GameObject, (Vector3, Quaternion)> Pins;
	public int FinalScore { get; private set; }
	public int ScoreFrame { get; set; }
	public bool BallDestroyed { get; set; }
	public FadeScreenManager FadeScreen { get; set; }
	public PauseMenuManager PauseManager { get; set; }
	public GameObject ReplayMenu { get; set; }

	private Transform _ballSpawnPoint;
	private Transform _pinsSpawnPoint;
	private int[] _frameScores;
	private int _frame;
	private int _round = 1;
	private int _throw = 1;
	private int _remainingPins = 10;
	private int _score;
	private bool _isPlayerOne;
	private bool _isStrike;
	private bool _isSpare;
	private bool _isSecondStrike;
	private bool _canThrowLast;
	private GameObject _scoreBoard;
	private GameObject _pins;
	private Color[] _colors;
	private TextMeshPro _currentScoreSlot;
	private TextMeshPro _previousScoreSlot;
	private TextMeshPro _twoStepsBackSlot;


	void Start()
	{
		_frameScores = new int[21];
		Pins = new();
		_isPlayerOne = PhotonNetwork.IsMasterClient;

		_scoreBoard = _isPlayerOne ? GameObject.Find("Screen Player 1") : GameObject.Find("Screen Player 2");
		_ballSpawnPoint = _isPlayerOne ? GameObject.Find("Ball Spawn Player 1").transform : GameObject.Find("Ball Spawn Player 2").transform;
		_pinsSpawnPoint = _isPlayerOne ? GameObject.Find("Pins Spawn Player 1").transform : GameObject.Find("Pins Spawn Player 2").transform;
		_pins = _isPlayerOne ? GameObject.FindWithTag("Pins1") : GameObject.FindWithTag("Pins2");

		PhotonView photonView = _pins.GetPhotonView(); ;
		photonView.ViewID = 0;
		PhotonNetwork.AllocateViewID(photonView);

		for (int i = 0; i < _pins.transform.childCount; i++)
		{
			Transform pin = _pins.transform.GetChild(i);
			Pins.Add(pin.gameObject, (pin.position, pin.rotation));
			pin.GetComponent<Pin>().enabled = true;
			photonView = pin.gameObject.GetPhotonView();
			photonView.ViewID = 0;
			PhotonNetwork.AllocateViewID(photonView);
		}
		_colors = new Color[]
		{
			Color.black,
			Color.blue,
			Color.cyan,
			Color.gray,
			Color.green,
			Color.red,
			Color.yellow,
			Color.magenta
		};
		SetBall();
	}


	private void Update()
	{
		if (BallDestroyed)
			StartCoroutine(ManageRound());
	}

	private IEnumerator ManageRound()
	{
		BallDestroyed = false;
		yield return new WaitForSeconds(2f);

		PauseManager.enabled = false;
		FadeScreen.FadeOut();
		yield return new WaitForSeconds(3.5f);

		if (_currentScoreSlot == null)
			_currentScoreSlot = _scoreBoard.transform.Find($"Turn {_round}/Final").GetComponent<TextMeshPro>();

		_frameScores[_frame++] = ScoreFrame;
		_remainingPins -= ScoreFrame;

		ManageStrikeAndSpare();
		CalculateScore();

		if (_round == 10 && _throw == 2 && _remainingPins == 0)
			_canThrowLast = true;

		WriteScore();

		if ((_isStrike || _isSpare) && _round != 10)
		{
			if (_isSecondStrike)
				_twoStepsBackSlot = _previousScoreSlot;
			if (_previousScoreSlot == null || _isSecondStrike)
				_previousScoreSlot = _currentScoreSlot;
		}

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

		ScoreFrame = 0;

		FadeScreen.FadeIn();
		yield return new WaitForSeconds(2f);
		PauseManager.enabled = true;
	}

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


	private void SetBall()
	{
		GameObject ball = PhotonNetwork.Instantiate("BowlingBall", _ballSpawnPoint.position, _ballSpawnPoint.rotation);
		ball.GetComponent<Renderer>().material.SetColor("_BaseColor", (Color)_colors.GetValue(Random.Range(0, _colors.Length)));

		PhotonView photonView = ball.GetPhotonView();
		photonView.ViewID = 0;
		PhotonNetwork.AllocateViewID(photonView);
	}

	private void SetPins()
	{
		_pins = PhotonNetwork.Instantiate(_isPlayerOne ? "PinsPlayer1" : "PinsPlayer2", _pinsSpawnPoint.position, _pinsSpawnPoint.rotation);
		PhotonView photonView = _pins.GetPhotonView();
		photonView.ViewID = 0;


		for (int i = 0; i < _pins.transform.childCount; i++)
		{
			Transform pin = _pins.transform.GetChild(i);
			Pins.Add(pin.gameObject, (pin.position, pin.rotation));
			photonView = pin.gameObject.GetPhotonView();
			photonView.ViewID = 0;
			PhotonNetwork.AllocateViewID(photonView);
		}
	}

	private void ResetPins()
	{
		foreach (KeyValuePair<GameObject, (Vector3, Quaternion)> kvp in Pins)
			kvp.Key.transform.SetPositionAndRotation(kvp.Value.Item1, kvp.Value.Item2);
		SetBall();
	}

	private void WriteScore()
	{
		TextMeshPro scoreSlot = _scoreBoard.transform.Find($"Turn {_round}/Phase {_throw}").GetComponent<TextMeshPro>();

		if (_throw == 1)
		{
			if (ScoreFrame == 0)
				scoreSlot.text = "-";
			else if (ScoreFrame == 10)
				scoreSlot.text = "X";
			else
				scoreSlot.text = ScoreFrame.ToString();
		}
		else
		{
			if (ScoreFrame == 0)
				scoreSlot.text = "-";
			else if (_remainingPins == 0 && _round == 10 && _isStrike)
				scoreSlot.text = "X";
			else if (_remainingPins == 0)
				scoreSlot.text = "/";
			else
				scoreSlot.text = ScoreFrame.ToString();

			if (!_isStrike && !_isSpare || _throw == 3)
				_currentScoreSlot.text = _score.ToString();
		}
	}

	private void ManagePins()
	{
		if ((_round == 10 && _throw == 2 && !_canThrowLast) || _throw == 3)
			FinishGame();
		else
		{
			Pins.Clear();
			PhotonNetwork.Destroy(_pins);
			SetPins();
			_remainingPins = 10;
			SetBall();
		}
		if (_round != 10)
		{
			_round++;
			_throw--;
			_currentScoreSlot = null;
		}
		else if (_round == 10 && _throw == 2 && _canThrowLast)
			_throw++;
	}

	private void ManageStrikeAndSpare()
	{
		if (_throw == 1)
		{
			if (_isSpare)
			{
				_previousScoreSlot.text = (_score - ScoreFrame).ToString();
				_previousScoreSlot = null;
			}
			if (_twoStepsBackSlot != null)
			{
				_twoStepsBackSlot.text = (_score - ScoreFrame * 2 - _frameScores[_frame - 2]).ToString();
				_twoStepsBackSlot = null;
			}
		}
		else
		{
			if (_isStrike && _previousScoreSlot != null)
			{
				_previousScoreSlot.text = (_score - ScoreFrame - _frameScores[_frame - 2]).ToString();
				_previousScoreSlot = null;
			}
		}

		if (_remainingPins == 0)
		{
			if (_throw == 1)
			{
				if (_isStrike)
					_isSecondStrike = true;
				_isStrike = true;
				_isSpare = false;
			}
			else
			{
				if (_round == 10 && _isStrike)
					_isStrike = true;
				else
				{
					_isStrike = false;
					_isSecondStrike = false;
					_isSpare = true;
				}
			}
		}
		else
		{
			if (_throw == 2)
				_isStrike = false;
			_isSecondStrike = false;
			_isSpare = false;
		}
	}

	private void FinishGame()
	{
		ReplayMenu.SetActive(true);
		GameManager.Instance.Player.GetComponent<InteractorManager>().ToggleMenuBehavior();
	}
}