using UnityEngine;

public class DropRateManager : MonoBehaviour
{
    public GameObject coin;
    public float dropRate;
    private void OnDestroy()
    {
        float randomNumber = Random.Range(0f, 100f);
        if (randomNumber <= dropRate)
        {
            Instantiate(coin, transform.position, Quaternion.identity);
        }
    }
}
