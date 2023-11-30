using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Collections;
using TMPro;

public class BowlingManager : MonoBehaviour
{
	public Dictionary<GameObject, (Vector3, Quaternion)> Pins;
	public int ScorePhase { get; set; }
	public bool BallDestroyed { get; set; }
	public FadeScreenManager FadeScreen { get; set; }
	private Transform _ballSpawnPoint;
	private Transform _pinsSpawnPoint;
	private int _turn = 1;
	private int _phase = 1;
	private int _score = 0;
	private int _remainingPins = 10;
	private bool _isPlayerOne;
	private GameObject _scoreBoard;
	private GameObject _pins;
	private Color[] _colors;

	void Start()
	{
		_isPlayerOne = PhotonNetwork.IsMasterClient;
		Pins = new();
		_scoreBoard = _isPlayerOne ? GameObject.Find("Screen Player 1") : GameObject.Find("Screen Player 2");
		_ballSpawnPoint = _isPlayerOne ? GameObject.Find("Ball Spawn Player 1").transform : GameObject.Find("Ball Spawn Player 2").transform;
		_pinsSpawnPoint = _isPlayerOne ? GameObject.Find("Pins Spawn Player 1").transform : GameObject.Find("Pins Spawn Player 2").transform;
		_pins = _isPlayerOne ? GameObject.FindWithTag("Pins1") : GameObject.FindWithTag("Pins2");
		for (int i = 0; i < _pins.transform.childCount; i++)
		{
			Transform pin = _pins.transform.GetChild(i);
			Pins.Add(pin.gameObject, (pin.position, pin.rotation));
			pin.GetComponent<Pin>().enabled = true;
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
	}


	private void Update()
	{
		if (BallDestroyed)
			StartCoroutine(ManageRound());
	}

	private IEnumerator ManageRound()
	{
		BallDestroyed = false;
		_score += ScorePhase;
		_remainingPins -= ScorePhase;

		FadeScreen.FadeOut();
		yield return new WaitForSeconds(2f);

		SetBall();
		WriteScore();
		if (_phase == 1)
		{
			_phase++;
			if (_remainingPins == 0)
			{
				_turn++;
				_phase--;
				Pins.Clear();
				Destroy(_pins);
				SetPins();
				_remainingPins = 10;
			}
			ResetPins();
		}
		else
		{
			_turn++;
			_phase--;
			Pins.Clear();
			Destroy(_pins);
			SetPins();
			_remainingPins = 10;
		}
		ScorePhase = 0;

		FadeScreen.FadeIn();
		yield return new WaitForSeconds(2f);
	}

	private void SetBall()
	{
		GameObject ball = PhotonNetwork.Instantiate("BowlingBall", _ballSpawnPoint.position, _ballSpawnPoint.rotation);
		ball.GetComponent<Renderer>().material.SetColor("_BaseColor", (Color)_colors.GetValue(Random.Range(0, _colors.Length)));
	}

	private void SetPins()
	{
		_pins = PhotonNetwork.Instantiate(_isPlayerOne ? "PinsPlayer1" : "PinsPlayer2", _pinsSpawnPoint.position, _pinsSpawnPoint.rotation);
		for (int i = 0; i < _pins.transform.childCount; i++)
		{
			Transform pin = _pins.transform.GetChild(i);
			Pins.Add(pin.gameObject, (pin.position, pin.rotation));
			pin.GetComponent<Pin>().enabled = true;
		}
	}

	private void ResetPins()
	{
		foreach (KeyValuePair<GameObject, (Vector3, Quaternion)> kvp in Pins)
			kvp.Key.transform.SetPositionAndRotation(kvp.Value.Item1, kvp.Value.Item2);
	}

	private void WriteScore()
	{
		TextMeshPro textMeshPro = _scoreBoard.transform.Find($"Turn {_turn}/Phase {_phase}").GetComponent<TextMeshPro>();
		if (_phase == 1)
		{
			if (ScorePhase == 0)
				textMeshPro.text = "-";
			else if (ScorePhase == 10)
				textMeshPro.text = "X";
			else
				textMeshPro.text = ScorePhase.ToString();
		}
		else
		{
			if (ScorePhase == 0)
				textMeshPro.text = "-";
			else if (_remainingPins == 0)
				textMeshPro.text = "/";
			else
				textMeshPro.text = ScorePhase.ToString();

			_scoreBoard.transform.Find($"Turn {_turn}/Final").GetComponent<TextMeshPro>().text = _score.ToString();
		}
	}
}
