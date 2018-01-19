using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameCtrl : MonoBehaviour
{
	//跳台下落速度
	public float Speed;

	//跳台
	public GameObject Platsform;

	//下一个跳台
	public GameObject NextPlatsform;

	//跳的物体
	public GameObject ChessAsset;

	//再来一次按钮
	public GameObject button;

	//得分
	public Text scoretext;

	//刚体
	public Rigidbody Rig;

	Vector3 Point1;

	Vector3 Point2;

	Vector3 PreChessPosition;

	float Timer;

	float Power;

	float Scale;

	float VSpeed;

	//是否按压中(应该与动画有关)
	bool IsPressing;

	//每跳跃一个格, 分数的技术
	int Bonus;

	//游戏状态
	InGameStatus status;

	bool IsToRight;

	GameObject Chess;

	GameObject ChessChild;

	//需要显示的跳台
	GameObject PrePlatsform;

	List<GameObject> Plats;

	CameraCtrl cameraCtrl;

	int Score;


	//按钮的重新开始方法
	public void SetRestart ()
	{
		button.SetActive (false);
		status = InGameStatus.init;  //游戏状态 - 初始化
	}

	//Methods
	Vector3 GetStartPoint ()
	{
		Debug.Log ("是否改变方向 ===>>> " + IsToRight);
		if (IsToRight) {
			return new Vector3 (0, 6, Random.Range (1.2f, 4));
		}
		return new Vector3 (-Random.Range (1.2f, 4), 6, 0);
	}

	// Use this for initialization
	void Start ()
	{

		button.SetActive (false);
		cameraCtrl = GetComponent< CameraCtrl > ();
		Plats = new List<GameObject> ();
		scoretext = scoretext.GetComponent<Text> ();

		Chess = Instantiate (ChessAsset, new Vector3 (0, 1.25f, 0), Quaternion.Euler (Vector3.zero));
		ChessChild = Chess.transform.Find ("default").gameObject;

		Vector3 position = new Vector3 (0, 0.5f, 0);
		NextPlatsform = Instantiate (Platsform, position, Quaternion.Euler (Vector3.zero));
		Plats.Add (NextPlatsform);
		status = InGameStatus.CreatePlat;  //游戏状态 - 创建Plat

		IsPressing = false;
		Power = 0;
		Bonus = 0;
		Score = 0;

		UpdateScore ();
	}
    
	// Update is called once per frame
	void Update ()
	{
		print (status);

		switch (status) {
            
		case InGameStatus.init: //如果游戏处于初始状态
			Score = 0;
			UpdateScore ();

			foreach (GameObject current in Plats) {  //销毁所以的木桩
				Destroy (current);
			}
			Destroy (Chess);  //销毁弹跳物体(象棋子)

                //跳台的位置
			Vector3 position = new Vector3 (0, 0.5f, 0);
			NextPlatsform = Instantiate (Platsform, position, Quaternion.Euler (Vector3.zero));
			Plats.Add (NextPlatsform);

			Chess = Instantiate (ChessAsset, new Vector3 (0, 1.25f, 0), Quaternion.Euler (Vector3.zero));
			ChessChild = Chess.transform.Find ("default").gameObject;

			cameraCtrl.CameraInit ();

			status = InGameStatus.CreatePlat;
			break;

		case InGameStatus.CreatePlat:  //如果游戏进入创建 plat 阶段
                
			PrePlatsform = NextPlatsform;  //需要显示的跳台

			IsToRight = (Random.Range (0, 10) > 5);
			NextPlatsform = Instantiate (Platsform, PrePlatsform.transform.position + GetStartPoint (), Quaternion.Euler (Vector3.zero));

			Plats.Insert (Plats.Count, NextPlatsform);

			Point1 = NextPlatsform.transform.position;
			Point2 = Point1;
			Point2.y = 0.5f;

            //重新设置相机的位置
			cameraCtrl.SetPosition ((PrePlatsform.transform.position + Point2) / 2);

			Timer = 0;
			status = InGameStatus.ShowPlat;
			Power = 0;
			break;
		case InGameStatus.ShowPlat:
                
			Timer += Time.deltaTime;
			NextPlatsform.transform.position = Vector3.Lerp (Point1, Point2, Timer * Speed);

			if (Timer * Speed > 1) {
				status = InGameStatus.TapInput;
				Timer = 0;
			}
			break;
		case InGameStatus.TapInput:  //按压时
			if (Input.anyKey) {
				IsPressing = true;
			}
			if (IsPressing) {
				if (Input.anyKey) {
					Timer += Time.deltaTime;
					if (Timer < 4) {
						Power = Timer * 3;
						PrePlatsform.transform.localScale = new Vector3 (1, 1 - 0.2f * Timer, 1);
						PrePlatsform.transform.Translate (0, -0.1f * Time.deltaTime, 0);
						Chess.transform.Translate (0, -0.2f * Time.deltaTime, 0);
					}
				} else {
					IsPressing = false;
					status = InGameStatus.BounceBack;
					Scale = PrePlatsform.transform.localScale.y;
				}
			}
			break;

		case InGameStatus.BounceBack:  //弹起
                
			Timer += Time.deltaTime;
			PrePlatsform.transform.localScale = Vector3.Lerp (new Vector3 (1, Scale, 1), new Vector3 (1, 1, 1), Timer);
			PrePlatsform.transform.Translate (0, 0.5f * Time.deltaTime, 0);
			Chess.transform.Translate (0, Time.deltaTime, 0);

			if (PrePlatsform.transform.position.y >= 0.5) {
				status = InGameStatus.ChessAnim;
				VSpeed = 0.3f;
				PreChessPosition = Chess.transform.position;
			}

				//跳完之后, 确保最多只有5个物体, 将之前多余的删除掉
			if (Plats.Count > 5) {
				//销毁不在视野范围内的物体
				Plats.RemoveAt (0);
				Destroy ((GameObject)Plats [0]);
			}
			break;
		case InGameStatus.ChessAnim:

			VSpeed -= Time.deltaTime;
			if (IsToRight) {
				Chess.transform.Translate (new Vector3 ((NextPlatsform.transform.position.x - PreChessPosition.x) / 0.6f * Time.deltaTime, VSpeed / 2, Power / 0.6f * Time.deltaTime));
				ChessChild.transform.Rotate (new Vector3 (600 * Time.deltaTime, 0));
			} else {
				Chess.transform.Translate (new Vector3 (-Power / 0.6f * Time.deltaTime, VSpeed / 2, (NextPlatsform.transform.position.z - PreChessPosition.z) / 0.6f * Time.deltaTime));
				ChessChild.transform.Rotate (new Vector3 (0, 0, 600 * Time.deltaTime));
			}
			if (Chess.transform.position.y <= 1) {
				Chess.transform.position = new Vector3 (Chess.transform.position.x, 1.25f, Chess.transform.position.z);
				ChessChild.transform.rotation = Quaternion.Euler (0, 0, 0);
				if (Mathf.Abs (Chess.transform.position.x - PrePlatsform.transform.position.x) < 0.5 && Mathf.Abs (Chess.transform.position.z - PrePlatsform.transform.position.z) < 0.5) {
					status = InGameStatus.TapInput;
				} else {
					if (Mathf.Abs (Chess.transform.position.x - NextPlatsform.transform.position.x) > 0.5 || Mathf.Abs (Chess.transform.position.z - NextPlatsform.transform.position.z) > 0.5) {
						status = InGameStatus.GameOver;
						Timer = 0;
					} else {
						if (Mathf.Abs (Chess.transform.position.x - NextPlatsform.transform.position.x) < 0.1 && Mathf.Abs (Chess.transform.position.z - NextPlatsform.transform.position.z) < 0.1) {
							Bonus++;
							Score += Bonus * 2;
							UpdateScore ();
						} else {
							Bonus = 0;
							Score++;
							UpdateScore ();
						}
						status = InGameStatus.CreatePlat;
					}
				}
			}
			break;

		case InGameStatus.GameOver:
			if (Timer == 0) {
				Rig = ChessChild.AddComponent<Rigidbody> ();
			}
			Timer += Time.deltaTime;
			if (Timer >= 1) {
				Timer = 0;
				status = InGameStatus.ShowMenu;
			}
			break;

		case InGameStatus.ShowMenu:
			button.SetActive (true);
			break;
		default:
			Debug.LogError ("这是什么状况~!");
			break;
		}

	}

	//跟新分数UI
	void UpdateScore ()
	{
		scoretext.text = Score.ToString ();
	}

	//
	// Nested Types
	//
	enum InGameStatus
	{
		init,
		//初始化
		CreatePlat,
		//创建
		ShowPlat,
		//显示
		TapInput,
		//点击
		BounceBack,
		//反弹
		ChessAnim,
		//动画
		GameOver,
		//游戏结束
		ShowMenu
		//显示菜单
	}
}
