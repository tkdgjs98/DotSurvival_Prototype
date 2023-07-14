using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

    public class ScoreRecord : NetworkBehaviour
    {
        [SyncVar]
        int score=0;
        [SerializeField]
        GamePlayerController player;
        
/*
        public ScoreRecord(GamePlayerController player)
        {
            this.score = 8;
            this.player = player;
        }
*/
        public int getScore()
        {
            return this.score;
        }
        public void PlusScore(int score)
        {
            this.score += score;
        }

        public GamePlayerController getPlayer()
        {
            return this.player;
        }
        public void setPlayer(GamePlayerController player)
        {
            this.player = player;
        }

        
    }
