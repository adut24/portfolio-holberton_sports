using Photon.Pun;

using UnityEngine;


public class BowlingStatusManager : MonoBehaviourPun
{
/*	private BowlingManager _bowlingManager;

	private void Start()
	{
		_bowlingManager = GameManager.Instance.BowlingManager;
		photonView.ObservedComponents.Add(this);
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			Debug.Log("Sending data");
			stream.SendNext(_bowlingManager.PlayerOneFinished);
			stream.SendNext(_bowlingManager.PlayerTwoFinished);
			stream.SendNext(_bowlingManager.Score);
		}
		else
		{
			Debug.Log("Receiving data");
			_bowlingManager.PlayerOneFinished = (bool)stream.ReceiveNext();
			_bowlingManager.PlayerTwoFinished = (bool)stream.ReceiveNext();
			_bowlingManager.OtherPlayerScore = (int)stream.ReceiveNext();
			Debug.Log("Received :" + _bowlingManager.PlayerOneFinished + " " + _bowlingManager.PlayerTwoFinished + " " + _bowlingManager.OtherPlayerScore);
		}
	}*/
}
