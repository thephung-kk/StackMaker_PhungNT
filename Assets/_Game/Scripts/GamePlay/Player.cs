using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Singleton<Player>
{
    [SerializeField] private float speed;
    [SerializeField] private LayerMask wallLayerMask;
    [SerializeField] private LayerMask brickLayerMask;
    [SerializeField] private LayerMask unbrickLayerMask;
    [SerializeField] private LayerMask pushLayerMask;
    [SerializeField] private LayerMask finishLayerMask;

    [SerializeField] private Transform raycastCheckpoint;
    [SerializeField] private Transform brickStack;
    [SerializeField] private Transform player;
    [SerializeField] private float playerHeight;

    [SerializeField] private GameObject brickPrefabs;
    [SerializeField] private Animator anim;
    private string curAnimName;

    private List<Transform> bricksList;
    private List<Brick> collectedBrick = new List<Brick>();
    private List<Unbrick> filledUnbrick = new List<Unbrick>();

    private Rigidbody rb;
    private Vector3 mouseDown, mouseUp;
    private bool isMove;
    private bool isPushed = true;
    private PlayerMoveDirection currentDirect;
    public GameState currentGameState = GameState.WaitToStart;
    public int score = 0;
    private int currentAnimState;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        //ChangeAnimationState(AnimState.Idle);
    }
    private void Update()
    {
            if (!isMove)
            {
                HandleMouseInput();
            }
            else
            {
                Move();
            }
            CheckCollider();
    }

    // Moving
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mouseDown = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            mouseUp = Input.mousePosition;
            currentDirect = GetDirect(mouseDown, mouseUp);
            if (currentDirect != PlayerMoveDirection.None)
            {
                isMove = true;
            }
        }
    }
    private PlayerMoveDirection GetDirect(Vector3 mouseDown, Vector3 mouseUp)
    {
        float delayX = mouseUp.x - mouseDown.x;
        float delayY = mouseUp.y - mouseDown.y;
        if (Vector3.Distance(mouseDown, mouseUp) < 100)
        {
            return PlayerMoveDirection.None;
        }
        else
        {
            if (Mathf.Abs(delayY) > Mathf.Abs(delayX))
            {
                if (delayY > 0)
                {
                    return PlayerMoveDirection.Forward;
                }
                else
                {
                    return PlayerMoveDirection.Backward;
                }
            }
            else
            {
                if (delayX > 0)
                {
                    return PlayerMoveDirection.Right;
                }
                else
                {
                    return PlayerMoveDirection.Left;
                }
            }
        }
    }
    public void Move()
    {
        if (currentGameState == GameState.InGame)
        {
            Vector3 direction = Vector3.zero;

            switch (currentDirect)
            {
                case PlayerMoveDirection.Forward:
                    direction = transform.forward;
                    break;
                case PlayerMoveDirection.Backward:
                    direction = -transform.forward;
                    break;
                case PlayerMoveDirection.Left:
                    direction = -transform.right;
                    break;
                case PlayerMoveDirection.Right:
                    direction = transform.right;
                    break;
                case PlayerMoveDirection.None:
                    isMove = false;
                    return;
                default:
                    break;
            }
            rb.velocity = direction * speed;
        }
    }
    private PlayerMoveDirection GetPushDirect(PlayerMoveDirection inputDirect)
    {
        switch (inputDirect)
        {
            case PlayerMoveDirection.Forward:
                if (Physics.Raycast(raycastCheckpoint.position, Vector3.right, 1f, wallLayerMask))
                {
                    return PlayerMoveDirection.Left;
                }
                else return PlayerMoveDirection.Right;
            case PlayerMoveDirection.Backward:
                if (Physics.Raycast(raycastCheckpoint.position, Vector3.right, 1f, wallLayerMask))
                {
                    return PlayerMoveDirection.Left;
                }
                else return PlayerMoveDirection.Right;
            case PlayerMoveDirection.Left:
                if (Physics.Raycast(raycastCheckpoint.position, Vector3.forward, 1f, wallLayerMask))
                {
                    return PlayerMoveDirection.Backward;
                }
                else return PlayerMoveDirection.Forward;
            case PlayerMoveDirection.Right:
                if (Physics.Raycast(raycastCheckpoint.position, Vector3.forward, 1f, wallLayerMask))
                {
                    return PlayerMoveDirection.Backward;
                }
                else return PlayerMoveDirection.Forward;
            default:
                return PlayerMoveDirection.None;
        }

    }
    private Vector3 GetMoveDirection(PlayerMoveDirection direct)
    {
        if (isMove)
        {
            switch (direct)
            {
                case PlayerMoveDirection.Forward:
                    return transform.forward;
                case PlayerMoveDirection.Backward:
                    return -transform.forward;
                case PlayerMoveDirection.Left:
                    return -transform.right;
                case PlayerMoveDirection.Right:
                    return transform.right;
                default:
                    return Vector3.zero;
            }
        }
        else return Vector3.zero;

    }
    // Check collider
    private void CheckCollider()
    {
        if (isMove)
        {
            if (CheckColliderWithLayer(pushLayerMask))
            {
                isPushed = false;
            }
            if (CheckColliderWithLayer(wallLayerMask))
            {
                if (isPushed) // stop if no push
                {
                    isMove = false;
                    rb.velocity = Vector3.zero;
                    FixPlayerPosition();
                }
                else // if have push, push in the push direction
                {
                    currentDirect = GetPushDirect(currentDirect);
                    isPushed = true;
                    Move();
                    FixPlayerPosition();
                }
            }
            // Finish level
            if (CheckColliderWithLayer(finishLayerMask))
            {
                StartCoroutine(CompleteLevel());
            }

        }
        CollectBrick();
        FillBrick();
    }

    private IEnumerator CompleteLevel()
    {
        isMove = false;
        rb.velocity = Vector3.zero;
        DestroyBrickStack();
        ChangeAnim("win");
        currentGameState = GameState.FinishLevel;
        ResetLevel();

        yield return new WaitForSeconds(0.5f);

        // last lvl
        if (LevelManager.Instance.currentLevel == LevelManager.Instance.levels.Length - 1)
        {
            UIManager.Instance.winGamePanel.SetActive(true);
            HighScoreManager.Instance.SaveHighScore(UIManager.Instance.username, Player.Instance.score,
                            UIManager.Instance.minutes, UIManager.Instance.seconds);
        }
        else
        {
            UIManager.Instance.completeLevelPanel.SetActive(true);
        }
    }

    private bool CheckColliderWithLayer(LayerMask layer)
    {
        Vector3 moveDirection = GetMoveDirection(currentDirect);
        return Physics.Raycast(raycastCheckpoint.position, moveDirection, 0.6f, layer);
    }


    // Collect and fill brick
    private void CollectBrick()
    {
        Vector3 moveDirection = GetMoveDirection(currentDirect);
        RaycastHit hit;
        Physics.Raycast(raycastCheckpoint.position, moveDirection, out hit, 0.5f, brickLayerMask);
        if (hit.collider != null)
        {
            Brick brick = hit.collider.GetComponent<Brick>();
            if (brick != null && brick.CanGetBrick())
            {
                StartCoroutine(PlayChangeHighAnimation());
                brick.PlayerGetBrick();
                score++;
                AddBrick();
                collectedBrick.Add(brick);
            }
        }
    }
    private void AddBrick()
    {
        GameObject brick = Instantiate(brickPrefabs, brickStack);
        if (bricksList == null)
        {
            bricksList = new List<Transform>();
        }
        bricksList.Add(brick.transform);
        UpdateIllustration();
    }
    private bool CanFillBrick()
    {
        return bricksList.Count > 0;
    }
    private void FillBrick()
    {
        Vector3 moveDirection = GetMoveDirection(currentDirect);
        RaycastHit hit;
        Physics.Raycast(raycastCheckpoint.position, moveDirection, out hit, 0.5f, unbrickLayerMask);
        if (hit.collider != null)
        {
            Unbrick unbrick = hit.collider.GetComponent<Unbrick>();
            if (unbrick == null)
            {
                return;
            }

            if (!unbrick.CanFill()) // bridge is already fill
            {
                return;
            }
            //if bridge is not fill and player doesn't have enough brick to fill
            if (!CanFillBrick())
            {
                HighScoreManager.Instance.SaveHighScore(UIManager.Instance.username, Player.Instance.score,
                                                        UIManager.Instance.minutes, UIManager.Instance.seconds);

                currentGameState = GameState.Lost;
                UIManager.Instance.lostPanel.SetActive(true);
                isMove = false;
                rb.velocity = Vector3.zero;
                ResetLevel();
                return;
            }
            StartCoroutine(PlayChangeHighAnimation());
            unbrick.PlayerFillBrick();
            score--;
            filledUnbrick.Add(unbrick);
            RemoveBrick();
        }
    }
    private void RemoveBrick()
    {
        if (bricksList.Count > 0)
        {
            Destroy(bricksList[bricksList.Count - 1].gameObject);
            bricksList.RemoveAt(bricksList.Count - 1);
        }
        UpdateIllustration();
    }
    private void DestroyBrickStack()
    {
        if (bricksList != null)
        {
            for (int i = bricksList.Count - 1; i >= 0; i--)
            {
                Destroy(bricksList[i].gameObject);
                bricksList.RemoveAt(i);
            }
            bricksList.Clear();
        }
        UpdateIllustration();
    }
    private void ResetLevel()
    {
        for (int i = 0; i < collectedBrick.Count; i++)
        {
            collectedBrick[i].ResetBrick();
            if (currentGameState == GameState.Lost)
            {
                score--;
            }
        }
        for (int i = 0; i < filledUnbrick.Count; i++)
        {
            filledUnbrick[i].ResetUnbrick();
            if (currentGameState == GameState.Lost)
            {
                score++;
            }
        }
        collectedBrick.Clear();
        filledUnbrick.Clear();
    }

    private void UpdateIllustration()
    {
        Vector3 pos = Vector3.zero;
        for (int i = 0; i < bricksList.Count; i++)
        {
            bricksList[i].localPosition = pos;
            pos += Vector3.up * 0.3f;
        }
        player.transform.localPosition = pos;
    }
    void FixPlayerPosition()
    {
        float numberX = (float)Math.Round(transform.position.x / 0.5f);
        float numberY = (float)Math.Round(transform.position.z / 0.5f);
        transform.position = new Vector3(numberX * 0.5f, transform.position.y, numberY * 0.5f);
    }
    public void ChangeAnim(string animName)
    {
        if (curAnimName != animName)
        {
            anim.ResetTrigger(animName);
            curAnimName = animName;
            anim.SetTrigger(curAnimName);
        }
    }
    private void SetIdleAnimation()
    {
        ChangeAnim("idle");
    }
    private IEnumerator PlayChangeHighAnimation()
    {
        ChangeAnim("changeHigh");
        yield return new WaitForSeconds(0.15f);  // Thời gian cho animation
        SetIdleAnimation();  // Trở về animation idle
    }
}
