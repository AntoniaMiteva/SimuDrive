using System.Collections;
using UnityEngine;

public class EndlessRoadScript : MonoBehaviour
{
	[SerializeField] private GameObject[] sectionPrefabs;
	private GameObject[] sectionsPool;
	private GameObject[] sections;

	private Transform playerCarTransform;
	private WaitForSeconds waitFor100ms = new WaitForSeconds(0.1f);

	private const float sectionLength = 196.7f;
	private int poolSize = 20;
	private int sectionsOnScreen = 10;

	void Start()
	{
		playerCarTransform = GameObject.FindGameObjectWithTag("Player").transform;

		sectionsPool = new GameObject[poolSize];
		sections = new GameObject[sectionsOnScreen];

		// Fill the object pool
		for (int i = 0; i < poolSize; i++)
		{
			int prefabIndex = Random.Range(0, sectionPrefabs.Length);
			sectionsPool[i] = Instantiate(sectionPrefabs[prefabIndex]);
			sectionsPool[i].SetActive(false);
		}

		// Place first sections
		for (int i = 0; i < sectionsOnScreen; i++)
		{
			GameObject section = GetRandomSectionFromPool();
			section.transform.position = new Vector3(0, 0, i * sectionLength);
			section.SetActive(true);
			sections[i] = section;
		}

		StartCoroutine(UpdateSections());
	}

	IEnumerator UpdateSections()
	{
		while (true)
		{
			for (int i = 0; i < sections.Length; i++)
			{
				if (sections[i].transform.position.z + sectionLength * 0.5f < playerCarTransform.position.z)
				{
					Vector3 lastPos = sections[i].transform.position;
					sections[i].SetActive(false);

					GameObject newSection = GetRandomSectionFromPool();
					newSection.transform.position = new Vector3(0, 0, lastPos.z + sectionLength * sections.Length);
					newSection.SetActive(true);
					sections[i] = newSection;
				}
			}
			yield return waitFor100ms;
		}
	}

	GameObject GetRandomSectionFromPool()
	{
		for (int i = 0; i < poolSize; i++)
		{
			int randomIndex = Random.Range(0, poolSize);
			if (!sectionsPool[randomIndex].activeInHierarchy)
			{
				return sectionsPool[randomIndex];
			}
		}

		// If no free section found (very rare), just instantiate a new one
		int fallbackIndex = Random.Range(0, sectionPrefabs.Length);
		return Instantiate(sectionPrefabs[fallbackIndex]);
	}
}
