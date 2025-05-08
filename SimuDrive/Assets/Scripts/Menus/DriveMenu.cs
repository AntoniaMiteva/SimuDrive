using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DriveMenu : MonoBehaviour
{
    public void BackButton()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void StartUpButton()
    {
        SceneManager.LoadScene("StartDriveScene");
    }

    public void StartUp1()
    {
        SceneManager.LoadScene("Start-Up-1");
    }
    public void StartUp2()
    {
        SceneManager.LoadScene("Start-Up-2");
    }
    public void StartUp3()
    {
        SceneManager.LoadScene("Start-Up-3");
    }

	public void ChangeGears1()
	{
		SceneManager.LoadScene("ChangeGears-1");
	}

	public void ChangeGears2()
	{
		SceneManager.LoadScene("ChangeGears-2");
	}

	public void Overtake1()
	{
		SceneManager.LoadScene("Overtake-1");
	}

	public void Overtake2()
	{
		SceneManager.LoadScene("Overtake-2");
	}

	public void RoundAbout1()
	{
		SceneManager.LoadScene("Round-About-1");
	}
	public void RoundAbout2()
	{
		SceneManager.LoadScene("Round-About-2");
	}
	public void RoundAbout3()
	{
		SceneManager.LoadScene("Round-About-3");
	}
	public void RoundAbout4()
	{
		SceneManager.LoadScene("Round-About-4");
	}
	public void RoadJunctionTrafficNoControl()
	{
		SceneManager.LoadScene("RoadJunctionTrafficNoControl");
	}
	public void RoadJunctionTrafficLight()
	{
		SceneManager.LoadScene("RoadJunctionTrafficLight");
	}
	public void RoadJunctionMainRoad()
	{
		SceneManager.LoadScene("RoadJunctionMainRoad");
	}
	public void RoadJunctionSecondRoad()
	{
		SceneManager.LoadScene("RoadJunctionSecondRoad");
	}
	public void RoadJunctionStop()
	{
		SceneManager.LoadScene("RoadJunctionStop");
	}
	public void Parking1()
	{
		SceneManager.LoadScene("Parking1");
	}
	public void Parking2()
	{
		SceneManager.LoadScene("Parking2");
	}
	public void OutOfCity()
	{
		SceneManager.LoadScene("OutOfCity");
	}
	public void OutOfCity2()
	{
		SceneManager.LoadScene("OutOfCity2");
	}
	public void PolygonFreeDriving()
	{
		SceneManager.LoadScene("PolygonFreeDriving");
	}
	public void CityFreeDriving()
	{
		SceneManager.LoadScene("CityFreeDriving");
	}
}
