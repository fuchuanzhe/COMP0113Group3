using UnityEngine;

public class PlayerOccupation : MonoBehaviour
{
    public enum Occupation
    {
        None,
        Chef,
        Butcher
    }

    public Occupation MyOccupation { get; private set; } = Occupation.None;
    public int MySeatIndex { get; private set; } = -1;

    public bool CanUseKnife => MyOccupation == Occupation.Chef;
    public bool CanUseHammer => MyOccupation == Occupation.Butcher;

    public void SetFromSeatIndex(int seatIndex)
    {
        MySeatIndex = seatIndex;
        MyOccupation = (seatIndex % 2 == 0) ? Occupation.Chef : Occupation.Butcher;
    }
}
