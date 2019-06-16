using System;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public GameObject waypointObject;

    [System.Serializable]
    public class Player
    {
        public enum Type
        {
            Any,
            Violet,
            Mint
        }

        bool human { get; set; }

        public Type type { get; set; }

        public Player(Type playerType)
        {
            type = playerType;
        }

        public GameObject spawnObject;
        public List<GameObject> spawnPoints = new List<GameObject>();
        public List<Card> cards = new List<Card>();
        public List<Pawn> pawns = new List<Pawn>();

        public Waypoint entry;
    }

    // States in game:
    // 1. Player need to pick a card from pile + 1 card every turn
    // 2. Player need to pick a card to play.
    // 3. If required player need to pick pawn on which card will be used.
    // 4. Check if player won. -> End game.
    // 5. Change player and start from top
    public enum GameState
    {
        TakeCardFromPile,
        PickCardAndPawnToPlay,
        PlayAction,
        ChangePlayer,
        FindWinner,
        GameOver,
        Unknown
    }

    public GameState gameState = GameState.Unknown;

    public Player currentPlayer = null;
    public Player lastPlayer = null;

    public Player mintPlayer = new Player(Player.Type.Mint);
    public Player violetPlayer = new Player(Player.Type.Violet);


    public List<Waypoint> walkableFields = new List<Waypoint>();

    CardPile cardPile;
    UnityEngine.UI.Text gameStateText;

    Card selectedCard = null;
    Pawn selectedPawn = null;

    public static Vector2 gridSize = new Vector2(120, 120);
    public static Vector2 boardSize = new Vector2(1920, 1080);

    private void Awake()
    {
        currentPlayer = mintPlayer;

        boardSize = GetComponent<Renderer>().bounds.size;
    }

    // Start is called before the first frame update
    private void Start()
    {
        cardPile = FindObjectOfType<CardPile>();
        gameStateText = FindObjectOfType<UnityEngine.UI.Text>();

        BuildBoardWaypoints();
        BuildSpawnPoints();
        CreatePlayerPawns();

        FindObjectOfType<AIPlayer>().player = violetPlayer;
    }

    private void Update()
    {
        if (currentPlayer != null
            && gameState != GameState.GameOver)
        {
            switch(gameState)
            {
                case GameState.Unknown:
                {

                    SetGameState(GameState.TakeCardFromPile);
                    break;
                }
                case GameState.TakeCardFromPile:
                {
                    if (currentPlayer.cards.Count >= 5)
                    {
                        SetGameState(GameState.PickCardAndPawnToPlay);
                    }

                    break;
                }
                case GameState.PickCardAndPawnToPlay:
                {
                    if (selectedCard != null && selectedPawn != null)
                    {
                        SetGameState(GameState.PlayAction);
                    }
                    else
                    {
                        SetGameState(GameState.PickCardAndPawnToPlay);
                    }

                    break;
                }
                case GameState.PlayAction:
                {
                    // Check if card and action match
                    if (selectedCard == null || selectedPawn == null)
                    {
                        SetGameState(GameState.PickCardAndPawnToPlay);
                        return;
                    }

                    Player opponenet = (currentPlayer == mintPlayer ? violetPlayer : mintPlayer);

                    if (opponenet.pawns.Contains(selectedPawn)
                            && selectedPawn.InGame()
                            && selectedCard.pawnSelector == Card.PawnSelector.InGamePawn
                            && selectedCard.sideSelector == Card.SideSelector.Opponenet)
                        {
                            OnPawnRemove(selectedPawn);
                        }
                    else if (selectedCard.sideSelector == Card.SideSelector.Own
                            && currentPlayer.pawns.Contains(selectedPawn))
                    {
                        if (selectedPawn.InSpawn()
                            && (selectedCard.pawnSelector == Card.PawnSelector.SpawnPawn
                            || selectedCard.pawnSelector == Card.PawnSelector.Both))
                        {
                            selectedPawn.position = currentPlayer.entry;
                        }
                        else if (selectedPawn.InGame()
                            && (selectedCard.pawnSelector == Card.PawnSelector.InGamePawn
                            || selectedCard.pawnSelector == Card.PawnSelector.Both))
                        {
                            if (selectedCard.direction == Card.Direction.Forward)
                            {
                                selectedPawn.Move(selectedCard.jumps, selectedCard.direction, currentPlayer.type);
                            }
                            else if (selectedCard.direction == Card.Direction.Back)
                            {
                                selectedPawn.Move(selectedCard.jumps, selectedCard.direction, currentPlayer.type);
                            }
                        }
                    }
                    else if (selectedCard.sideSelector == Card.SideSelector.Opponenet
                            && opponenet.pawns.Contains(selectedPawn))
                    {
                    }

                    if (selectedPawn.Finished() && currentPlayer.pawns.Contains(selectedPawn))
                    {
                            selectedPawn.Disabled();
                            currentPlayer.pawns.Remove(selectedPawn);
                    }

                    cardPile.ReturnCard(selectedCard);
                    currentPlayer.cards.Remove(selectedCard);

                    // We are done clear selected objects
                    selectedCard = null;
                    selectedPawn = null;

                    SetGameState(GameState.FindWinner);

                    break;
                }
                case GameState.FindWinner:
                {
                        if (violetPlayer.pawns.Count == 0
                            || mintPlayer.pawns.Count == 0)
                        {
                            SetGameState(GameState.GameOver);
                        }
                        else
                        {
                            SetGameState(GameState.ChangePlayer);
                        }

                        break;
                }
                case GameState.ChangePlayer:
                    currentPlayer = (currentPlayer == mintPlayer ? violetPlayer : mintPlayer);
                    SetGameState(GameState.TakeCardFromPile);

                    break;
                default:
                {
                    break;
                }
                //case GameState.PickPawnToPlay:
                //{
                //        //if (selectedCard.sideSelector == Card.SideSelector.Opponenet)
                //        //{
                //        //    Player opponent = violetPlayer;
                //        //    if (currentPlayer == violetPlayer)
                //        //        currentPlayer = mintPlayer;
                //        //}

                //        //if (selectedCard.direction == Card.Direction.None
                //        //    && selectedCard.pawnSelector)
                //        //{
                //        //}
                //    break;
                //}
            }
        }
    }

    void BuildSpawnPoints()
    {
        // Mint spawn points
        mintPlayer.spawnPoints.Add(Instantiate(mintPlayer.spawnObject, MapToUnityPosition(MapPositionToGridPosition(1, 1)), Quaternion.identity));
        mintPlayer.spawnPoints.Add(Instantiate(mintPlayer.spawnObject, MapToUnityPosition(MapPositionToGridPosition(2, 1)), Quaternion.identity));
        mintPlayer.spawnPoints.Add(Instantiate(mintPlayer.spawnObject, MapToUnityPosition(MapPositionToGridPosition(1, 2)), Quaternion.identity));
        mintPlayer.spawnPoints.Add(Instantiate(mintPlayer.spawnObject, MapToUnityPosition(MapPositionToGridPosition(2, 2)), Quaternion.identity));

        mintPlayer.spawnPoints.ForEach(gameObject => gameObject.transform.parent = transform);

        // Mint spawn points
        violetPlayer.spawnPoints.Add(Instantiate(violetPlayer.spawnObject, MapToUnityPosition(MapPositionToGridPosition(13, 6)), Quaternion.identity));
        violetPlayer.spawnPoints.Add(Instantiate(violetPlayer.spawnObject, MapToUnityPosition(MapPositionToGridPosition(14, 6)), Quaternion.identity));
        violetPlayer.spawnPoints.Add(Instantiate(violetPlayer.spawnObject, MapToUnityPosition(MapPositionToGridPosition(13, 7)), Quaternion.identity));
        violetPlayer.spawnPoints.Add(Instantiate(violetPlayer.spawnObject, MapToUnityPosition(MapPositionToGridPosition(14, 7)), Quaternion.identity));

        violetPlayer.spawnPoints.ForEach(gameObject => gameObject.transform.parent = transform);
    }

    void CreatePlayerPawns()
    {
        // Now create player pawns
        for (int i = 0; i < mintPlayer.spawnPoints.Count; ++i)
        {
            Pawn pawn = mintPlayer.spawnPoints[i].GetComponent<SpawnPoint>().CreatePawn();
            mintPlayer.pawns.Add(pawn);
        }

        // Now create player pawns
        for (int i = 0; i < violetPlayer.spawnPoints.Count; ++i)
        {
            Pawn pawn = violetPlayer.spawnPoints[i].GetComponent<SpawnPoint>().CreatePawn();
            violetPlayer.pawns.Add(pawn);
        }
    }

    void BuildBoardWaypoints()
    {
        Waypoint waypointPrev = null;
        Waypoint waypointCurrent = null;

        // Predefined walkable fields for current map
        //
        // top: left -> right
        for (int i = 3; i < 14; ++i)
        {
            AddWaypoint(i, 1, ref waypointCurrent, ref waypointPrev);
        }

        // right: top -> bottom
        for (int i = 1; i < 4; ++i)
        {
            AddWaypoint(14, i, ref waypointCurrent, ref waypointPrev);
        }

        // Save waypoint for later
        Waypoint waypointPrevTemp = waypointPrev;
        Waypoint waypointCurrentTemp = waypointCurrent;

        // build Violet home path
        for (int i = 13; i > 8; --i)
        {
            AddWaypoint(i, 3, ref waypointCurrent, ref waypointPrev);
            waypointCurrent.UseBy(Player.Type.Violet);
        }

        // Restore waypoints
        waypointPrev = waypointPrevTemp;
        waypointCurrent = waypointCurrentTemp;

        // right: top -> bottom
        for (int i = 4; i < 5; ++i)
        {
            AddWaypoint(14, i, ref waypointCurrent, ref waypointPrev);
        }


        for (int i = 14; i > 12; --i)
        {
            AddWaypoint(i, 5, ref waypointCurrent, ref waypointPrev);
        }

        violetPlayer.entry = waypointCurrent;

        for (int i = 5; i < 7; ++i)
        {
            AddWaypoint(12, i, ref waypointCurrent, ref waypointPrev);
        }

        // bottom: right -> left
        for (int i = 12; i > 1; --i)
        {
            AddWaypoint(i, 7, ref waypointCurrent, ref waypointPrev);
        }

        // left: bottom -> top
        for (int i = 7; i > 4; --i)
        {
            AddWaypoint(1, i, ref waypointCurrent, ref waypointPrev);
        }

        // Save waypoint for later
        waypointPrevTemp = waypointPrev;
        waypointCurrentTemp = waypointCurrent;

        // build Violet home path
        for (int i = 2; i < 7; ++i)
        {
            AddWaypoint(i, 5, ref waypointCurrent, ref waypointPrev);
            waypointCurrent.UseBy(Board.Player.Type.Mint);
        }

        // Restore waypoints
        waypointPrev = waypointPrevTemp;
        waypointCurrent = waypointCurrentTemp;

        AddWaypoint(1, 4, ref waypointCurrent, ref waypointPrev);

        for (int i = 1; i < 3; ++i)
        {
            AddWaypoint(i, 3, ref waypointCurrent, ref waypointPrev);
        }

        mintPlayer.entry = waypointCurrent;

        for (int i = 3; i > 1; --i)
        {
            AddWaypoint(3, i, ref waypointCurrent, ref waypointPrev);
        }

        // Connect with begine
        waypointCurrent.AddNextWaypoint(walkableFields[0]);
        walkableFields[0].AddPreviousWaypoint(waypointCurrent);
    }

    void AddWaypoint(int x, int y, ref Waypoint waypointCurrent, ref Waypoint waypointPrevious)
    {
        GameObject gameObject = Instantiate(waypointObject, MapToUnityPosition(MapPositionToGridPosition(x, y)), Quaternion.identity);
        waypointCurrent = gameObject.GetComponent<Waypoint>();
        if (waypointPrevious != null)
        {
            waypointPrevious.AddNextWaypoint(waypointCurrent);
            waypointCurrent.AddPreviousWaypoint(waypointPrevious);
        }
        walkableFields.Add(waypointCurrent);
        waypointPrevious = waypointCurrent;
        waypointCurrent.transform.parent = transform;
    }

    Vector2 MapPositionToGridPosition(int x, int y)
    {
        return new Vector2(gridSize.x * x, gridSize.y * y);
    }

    Vector3 MapToUnityPosition(Vector2 position)
    {
        return new Vector3(-boardSize.x / 2 + gridSize.x / 2 + position.x, boardSize.y / 2 - gridSize.y / 2 - position.y, 0);
    }


    // banch of callbacks from other classes
    // returning true/false depensing on user move and game state

    // Fail when someone click on pile and card from pile will be delivered here.
    // if return false card should go back to pile.

    public bool OnCardRequest(Card card, bool externalEvent)
    {
        if (externalEvent && currentPlayer == violetPlayer)
            return false;

        if (gameState == GameState.TakeCardFromPile
            && currentPlayer.cards.Count < 5)
        {
            card.Enabled();
            currentPlayer.cards.Add(card);
            return true;
        }

        return false;
    }

    public bool OnCardSelected(Card card, bool externalEvent)
    {
        if (externalEvent && currentPlayer == violetPlayer)
            return false;

        if (currentPlayer.cards.Contains(card)
            && gameState == GameState.PickCardAndPawnToPlay)
        {
            selectedCard = card;

            return true;
        }

        return false;
    }

    public bool OnPawnSelected(Pawn pawn, bool externalEvent)
    {
        if (externalEvent && currentPlayer == violetPlayer)
            return false;


        if (gameState == GameState.PickCardAndPawnToPlay)
        {
            selectedPawn = pawn;

            return true;
        }

        return false;
    }

    public void OnPawnRemove(Pawn needToBeRemoved)
    {
        if (currentPlayer.pawns.Contains(needToBeRemoved))
            return;

        if (mintPlayer.pawns.Contains(needToBeRemoved))
        {
            List<GameObject> points = mintPlayer.spawnPoints;
            List<GameObject> removed_points = new List<GameObject>();

            foreach (GameObject obj in points)
            {
                foreach (Pawn pawn in mintPlayer.pawns)
                {
                    if (pawn.transform.position.x == obj.transform.position.x
                            && pawn.transform.position.y == obj.transform.position.y)
                    {
                        removed_points.Add(obj);
                    }
                }
            }

            foreach (GameObject obj in removed_points)
            {
                points.Remove(obj);
            }

            needToBeRemoved.transform.position = new Vector3(points[0].transform.position.x, points[0].transform.position.y, needToBeRemoved.transform.position.z);
            Pawn playerPawn = needToBeRemoved.gameObject.GetComponent<Pawn>();
            playerPawn.position = null;
        }

        if (violetPlayer.pawns.Contains(needToBeRemoved))
        {
            List<GameObject> points = violetPlayer.spawnPoints;
            List<GameObject> removed_points = new List<GameObject>();

            foreach (GameObject obj in points)
            {
                foreach (Pawn pawn in violetPlayer.pawns)
                {
                    if (pawn.transform.position.x == obj.transform.position.x
                            && pawn.transform.position.y == obj.transform.position.y)
                    {
                        removed_points.Add(obj);
                    }
                }
            }

            foreach (GameObject obj in removed_points)
            {
                points.Remove(obj);
            }

            needToBeRemoved.transform.position = new Vector3(points[0].transform.position.x, points[0].transform.position.y, needToBeRemoved.transform.position.z);
            Pawn playerPawn = needToBeRemoved.gameObject.GetComponent<Pawn>();
            playerPawn.position = null;
        }
    }

    // print text from state of game

    private void SetGameState(GameState state)
    {
        switch(state)
        {
            case GameState.GameOver:
                gameStateText.text = "Game over";

                if (violetPlayer.pawns.Count == 0)
                {
                    gameStateText.text += ". Violet player won!";
                }
                else if (mintPlayer.pawns.Count == 0)
                {
                    gameStateText.text += ". Mint player won!";
                }
                break;
            case GameState.TakeCardFromPile:
                gameStateText.text = "Take cards from pile";
                break;
            case GameState.PickCardAndPawnToPlay:
                gameStateText.text = "Pick ";
                if (selectedPawn == null)
                    gameStateText.text += "pawn";
                if (selectedCard == null && selectedPawn == null)
                    gameStateText.text += " and ";
                if (selectedCard == null)
                    gameStateText.text += "card";
                gameStateText.text += " to play";
                break;
            case GameState.PlayAction:
                gameStateText.text = "Playing action";
                break;
            case GameState.ChangePlayer:
                gameStateText.text = "Waiting for player X to play";
                break;
            default:
                break;
        }

        gameState = state;
    }
}
