using Photon.Pun;

using TMPro;

/// <summary>
/// Responsible for writing the score on the score boards in all clients.
/// </summary>
public class ScoreWriter : MonoBehaviourPun
{
    private TextMeshPro _currentScoreSlot;
    private TextMeshPro _previousScoreSlot;
    private TextMeshPro _twoStepsBackScoreSlot;
    private bool _isSpare;
    private bool _isStrike;
    private bool _isSecondStrike;

    /// <summary>
    /// Sets the final slot of the round.
    /// </summary>
    /// <param name="round">Current round</param>
    public void SetCurrentSlot(int round)
    {
        if (_currentScoreSlot == null)
            photonView.RpcSecure("SetCurrentSlotOnClients", RpcTarget.All, true, round);
    }

    /// <summary>
    /// Sets the final slot of the round for the local player on all clients.
    /// </summary>
    /// <param name="round">Current round</param>
    [PunRPC]
    private void SetCurrentSlotOnClients(int round) => _currentScoreSlot = transform.Find($"Turn {round}/Final").GetComponent<TextMeshPro>();

    /// <summary>
    /// Removes the final slot of the round.
    /// </summary>
    public void ResetCurrentSlot() => photonView.RpcSecure("ResetCurrentSlotOnClients", RpcTarget.All, true);

    /// <summary>
    /// Removes the final slot of the round for the local on all clients.
    /// </summary>
    [PunRPC]
    private void ResetCurrentSlotOnClients() => _currentScoreSlot = null;

    /// <summary>
    /// Updates the final round fields.
    /// </summary>
    /// <param name="round">Current round</param>
    public void UpdateScoreSlots(int round) => photonView.RpcSecure("UpdateScoreSlotsOnClients", RpcTarget.All, true, round);

    /// <summary>
    /// Updates the final round fields of the local player on all clients.
    /// </summary>
    /// <param name="round"></param>
    [PunRPC]
    private void UpdateScoreSlotsOnClients(int round)
    {
        if ((_isStrike || _isSpare) && round != 10)
        {
            if (_isSecondStrike)
                _twoStepsBackScoreSlot = _previousScoreSlot;
            if (_previousScoreSlot == null || _isSecondStrike)
                _previousScoreSlot = _currentScoreSlot;
        }
    }

    /// <summary>
    /// Writes the score of the player on the score board.
    /// </summary>
    /// <param name="score">Current score of the player</param>
    /// <param name="round">Current round of the player</param>
    /// <param name="throwNumber">Number of the throw in the current round</param>
    /// <param name="scoreFrame">Score of the current throw</param>
    /// <param name="remainingPins">Number of pins remaining up after the throw</param>
    public void WriteScore(string score, int round, int throwNumber, int scoreFrame, int remainingPins)
    {
        photonView.RpcSecure("WriteScoreOnClients", RpcTarget.All, true, score, round, throwNumber, scoreFrame, remainingPins);
    }

    /// <summary>
    /// Writes the score of the player on the score board on all clientss.
    /// </summary>
    /// <param name="score">Current score of the player</param>
    /// <param name="round">Current round of the player</param>
    /// <param name="throwNumber">Number of the throw in the current round</param>
    /// <param name="scoreFrame">Score of the current throw</param>
    /// <param name="remainingPins">Number of pins remaining up after the throw</param>
    [PunRPC]
    private void WriteScoreOnClients(string score, int round, int throwNumber, int scoreFrame, int remainingPins)
    {
        TextMeshPro scoreSlot = transform.Find($"Turn {round}/Phase {throwNumber}").GetComponent<TextMeshPro>();

        if (throwNumber == 1)
        {
            if (scoreFrame == 0)
                scoreSlot.text = "-";
            else if (scoreFrame == 10)
                scoreSlot.text = "X";
            else
                scoreSlot.text = scoreFrame.ToString();
        }
        else
        {
            if (scoreFrame == 0)
                scoreSlot.text = "-";
            else if (remainingPins == 0 && round == 10 && _isStrike)
                scoreSlot.text = "X";
            else if (remainingPins == 0)
                scoreSlot.text = "/";
            else
                scoreSlot.text = scoreFrame.ToString();

            if (!_isStrike && !_isSpare || throwNumber == 3)
                _currentScoreSlot.text = score;
        }
    }

    /// <summary>
    /// Manages the strike and spare states.
    /// </summary>
    /// <param name="throwNumber">Number of the throw</param>
    /// <param name="score">Current score of the player</param>
    /// <param name="frame">Current frame of the player</param>
    /// <param name="scoreFrame">Score at the current of frame</param>
    /// <param name="remainingPins">Number of pins remaining up after the throw</param>
    /// <param name="round">Current round of the player</param>
    /// <param name="frameScores">Scores done each frame of the game</param>
    public void ManageStrikeAndSpare(int throwNumber, int score, int frame, int scoreFrame, int remainingPins, int round, int[] frameScores)
    {
        photonView.RpcSecure("ManageStrikeAndSpareOnClients", RpcTarget.All, true, throwNumber, score, frame, scoreFrame, remainingPins, round, frameScores);
    }

    /// <summary>
    /// Manages the strike and spare states of the local player on all clients.
    /// </summary>
    /// <param name="throwNumber">Number of the throw</param>
    /// <param name="score">Current score of the player</param>
    /// <param name="frame">Current frame of the player</param>
    /// <param name="scoreFrame">Score at the current of frame</param>
    /// <param name="remainingPins">Number of pins remaining up after the throw</param>
    /// <param name="round">Current round of the player</param>
    /// <param name="frameScores">Scores done each frame of the game</param>
    [PunRPC]
    private void ManageStrikeAndSpareOnClients(int throwNumber, int score, int frame, int scoreFrame, int remainingPins, int round, int[] frameScores)
    {
        if (throwNumber == 1)
        {
            /*Checks if the previous throw was a spare or not */
            if (_isSpare)
            {
                _previousScoreSlot.text = (score - scoreFrame).ToString();
                _previousScoreSlot = null;
            }
            /*Checks if the 2 previous throws were a strike or not */
            if (_twoStepsBackScoreSlot != null)
            {
                _twoStepsBackScoreSlot.text = (score - scoreFrame * 2 - frameScores[frame - 2]).ToString();
                _twoStepsBackScoreSlot = null;
            }
        }
        else
        {
            /* Checks if previous frame was a strike or not */
            if (_isStrike && _previousScoreSlot != null)
            {
                _previousScoreSlot.text = (score - scoreFrame - frameScores[frame - 2]).ToString();
                _previousScoreSlot = null;
            }
        }

        if (remainingPins == 0)
        {
            if (throwNumber == 1)
            {
                /* Checks if it's the second strike in a row */
                if (_isStrike)
                    _isSecondStrike = true;
                _isStrike = true;
                _isSpare = false;
            }
            else
            {
                if (round == 10 && _isStrike)
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
            /* Resets all variables as not all pins were knocked down */
            if (throwNumber == 2)
                _isStrike = false;
            _isSecondStrike = false;
            _isSpare = false;
        }
    }
}
