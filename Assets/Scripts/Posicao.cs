﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Posicao : MonoBehaviour {
	public GameObject peca;
	// Use this for initialization
	void Start () {
		if(peca){
			Vector3 aux = gameObject.transform.position;
			aux.z -= 2;
			peca.transform.position = aux;
		}
	}
}
