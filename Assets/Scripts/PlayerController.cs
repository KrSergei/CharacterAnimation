using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Поля

    public Transform UpperRaycastSpot;          //Трансформ для позиции верхнего луча



    public float FirstLine,                     //Указатель на позицию первой линии (линия номер 0)
                 LineDistance;                  //Расстояние между линиями
    public float PlayerSpeed = 5.0f,            //Скорость игрока
                 DistanceGround,                //Дистанция луча hit, которая указывает, что игрок находится на поверхности
                 GravytyForce,                  //Показатель силы гравитации
                 DistanceMovToSide = 3f,        //Расстояние, которое проходит игрок за одну анимацию кувырка в сторону
                 DistanceMoveToForward = 10f,   //Расстояние, которое проходит игрок за одну анимацию кувырка или прыжка по ходу движения
                 ClimbingDistance,              //Расстояние перемещеиния при взбирании на вертикальную поверхность
                 JumpForce,                     //Сила прыжка
                 JumpClimbForce;                //Сила прыжка при анимации взбирания на стену

    public bool IsInMovement = false,           //Указатель состояния выполнения анимации
                IsGround = false,
                IsClimbing = false;
    public LayerMask LayerMaskGround,
                     LayerMaskWall;

    private Animator animator;
    private CharacterController cc;
    private Vector3 runVector,                  //Вектор направления движения персонажа 
                    moveVector,                 //Вектор направления движения персонажа при выполении перемещения
                    gravity;                    //Вектор направления гравитации

    private Vector3 ccCenterNorm = new Vector3(0, .91f, 0),             //Вектор центра коллайдера контроллера по умоланию          
                    ccCenterRoll = new Vector3(0, .19f, 0),             //Вектор центра коллайдера контроллера при анимации кувырка
                    ccCenterJump = new Vector3(0, 1.6f, 0),             //Вектор центра коллайдера контроллера при анимации прыжка
                    ccCenterClimb = new Vector3(0, .91f, -0.2f);        //Вектор центра коллайдера контроллера при анимации взбирания на землю


    private RaycastHit hitGround,
                       hitForward; 

    private float timeAnimationClip,            //Время длительности анимационного клипа
                  currentDistance = 0f,         //Текущая дистанция
                  currentDirection = 0f,        //Текущее направление   
                  currentSpeed,                 //Текущая скорость 
                  tmpDist,                      //Дистанция, пройденная за один кадр анимации 
                  chooseDistance;               //Показатель дистанции, на которую необходимо передвинуть персонажа в зависимости от выбранного направления


    private float directionSide,                //Направление управления влево-вправо
                  directionForward,             //Направление управления вниз-вверх
                  currentJumpForce;             //Текущий показатель силы прыжка в зависимости от анимации

    private string nameWorkedTrigger;

    private float ccHeightHorm = 1.91f,                   //Высота коллайдера контроллера по умолчанию
                  ccHeightForRollAndJump = .4f;           //Высота коллайдера контроллера при кувырке и прыжке 

    private int lineNumber = 1,                 //Указатель номера линии на которой находится игрок, вначале игры на линии №1
                linesCount = 2;                 //Количество линий, всего три линии (с номерами 0, 1 и 2)

    private bool isMovementClimbing = false;


    #endregion

    void Start()
    {
        //Определения значения Аниматора
        animator = GetComponent<Animator>();
        //Определение значения контроллера
        cc = GetComponent<CharacterController>();
        //Определение вектора движения
        runVector = Vector3.forward;
    }

    void Update()
    {
        //Направляем луч вниз, с позиции игрока 
        Ray rayGround = new Ray(transform.position, -Vector3.up);
        Ray rayUpper = new Ray(UpperRaycastSpot.position, Vector3.forward);
        Physics.Raycast(rayGround, out hitGround, LayerMaskGround);
        Physics.Raycast(rayUpper, out hitForward, LayerMaskWall);

        //Определение высоты игрока, если дистанция луча меньше либо равна расстоянию distanceGround, то isGround = true, иначе isGround = false
        if (hitGround.distance <= DistanceGround)
        {
            IsGround = true;
            gravity = Vector3.zero;
            directionSide = Input.GetAxisRaw("Horizontal");
            directionForward = Input.GetAxisRaw("Vertical");
        }
        else
        {
            IsGround = false;
            gravity = Physics.gravity * Time.deltaTime * GravytyForce;
        }

        if(hitForward.distance <= 3f)
            IsClimbing = true;
        else
            IsClimbing = false;

        if (!IsInMovement)
        {
            if (directionSide != 0 || directionForward != 0)
            {
                IsInMovement = true;
                if (directionSide < 0)
                    nameWorkedTrigger = "Left";
                if (directionSide > 0)
                    nameWorkedTrigger = "Right";
                if (directionForward < 0)
                    nameWorkedTrigger = "Roll";

                //Если при directionForward > 0 и IsClimbing = true, установка переменной nameWorkedTrigger для проигрывания анимации взбирания на стену
                //установка силы прыжка и расстояние перемещения при анимации взбирания на стену
                if (directionForward > 0 && IsClimbing)
                {
                    nameWorkedTrigger = "Climb";
                    currentJumpForce = JumpClimbForce;
                    currentDistance = ClimbingDistance;
                    //Возврат флага IsClimbing = false
                    IsClimbing = false;
                }
                else if (directionForward > 0)
                {
                    currentJumpForce = JumpForce;
                    //Установка текущей дистанции равной длине перемещения для анимации прыжка
                    currentDistance = DistanceMoveToForward;
                    nameWorkedTrigger = "Jump";
                }

                //Определение вектора направления движения
                if (directionSide != 0)
                {
                    //Установка направления передвижения в зависимости от значения directionSide, если больше 0 - движение вправо, если меньше 0 - движение влево
                    currentDirection = directionSide;
                    //Установка текущей дистанции равной длине перемещения
                    currentDistance = DistanceMovToSide;
                    //Установка вектора движения
                    moveVector = Vector3.right;
                }

                if (directionForward != 0)
                {

                    //Установка направления движения по оси Z по модулю, что бы персонаж не двигался назад в случпе, если directionForward < 0
                    currentDirection = Mathf.Abs(directionForward);

                    Debug.Log("Distance - " + currentDistance);

                    Debug.Log("СurrentJumpForce - " + currentJumpForce);

                    //Установка вектора движения
                    moveVector = Vector3.forward;
                }
                //Установка триггера с именем nameWorkedTrigger
                animator.SetTrigger(nameWorkedTrigger);
            }
        }

        if (isMovementClimbing)
        {
            Debug.Log("Worked");
            runVector.z = 0;
            runVector.y += 2f;
            runVector *= Time.deltaTime;
            cc.Move(runVector);
        }

        //Задание движения персонажу
        runVector.z += PlayerSpeed;
        runVector += gravity;
        runVector *= Time.deltaTime;

        //Вызом метода Move для выполения соответвующей анимации
        if (IsInMovement)
        {
            Move();
        }
        cc.Move(runVector);
    }

    /// <summary>
    /// Метод реализации перемещения игрока
    /// </summary>
    private void Move()
    {
        //Получение длины текущего проигрываеммой анимации
        timeAnimationClip = animator.GetCurrentAnimatorStateInfo(0).length;

        //Если текущая пройденная дистанция меньше, либо равна нулю, установка isInMovement = false и выходи из метода
        if (currentDistance <= 0)
        {
            IsInMovement = false;
            return;
        }

        if (isMovementClimbing)
        {
            Debug.Log("Worked");
            runVector.z = 0;
            runVector.y += 2f;
            runVector *= Time.deltaTime;
            //cc.Move(runVector);
        }


        //Выбор расстояния, на которое необхоидмо пеерместить персонаж, если directionForward не равно 0, расстояние перемещения равно DistanceMoveToForward
        //иначе chooseDistance = DistanceMovToSide
        if (directionForward != 0)
            chooseDistance = DistanceMoveToForward;
        else
            chooseDistance = DistanceMovToSide;

        //Вычисление текущей скорости персонажа
        currentSpeed = chooseDistance / timeAnimationClip;

        //Дистанция, которую проходит персонаж за один кадр
        tmpDist = Time.deltaTime * currentSpeed;


        //Задание вектору движения гравитации
        moveVector += gravity;

        //Задание вектора движения в зависимости от текущего направления и пройденной дистанции 
        runVector = moveVector * currentDirection * tmpDist;

        //Уменьшение текущей дистанции на пройденную дистанцию
        currentDistance -= tmpDist;
    }

    //Добавление к вектору движения персонажа текущую силу прыжка  по событию из анимации
    private void AnimationEventJump() => moveVector.y = currentJumpForce;

    /// <summary>
    /// Метод для установки центра и высоты коллайдера для анимации кувырка. Вызывается по событию в анимации кувырка
    /// </summary>
    private void AnimationEventRoll()
    {
        //Изменение высоты коллайдера для кувырка
        cc.height = ccHeightForRollAndJump;
        //Изменение центра коллайдера для кувырка
        cc.center = ccCenterRoll;
    }

    /// <summary>
    /// Метод для установки центра и высоты коллайдера для анимации прыжка. Вызывается по событию в анимации прыжка
    /// </summary>
    private void SetCollaiderForJump()
    {
        ////Изменение высоты коллайдера для прыжка
        cc.height = ccHeightForRollAndJump;
        ////Изменение центра коллайдера для прыжка
        cc.center = ccCenterJump;
    }

    private void SetCollaiderForClimbp()
    {
        //Изменение центра коллайдера для прыжка
        cc.center = ccCenterClimb;
    }

    private void AnimationEventActivationClimb() => isMovementClimbing = true;

    /// <summary>
    /// Метод для сброса центра и высоты коллайдера после анимации кувырка. Вызывается по событию в анимации
    /// </summary>
    private void SetNormalCollaider()
    {
        //Сброс центра и высоты коллайдера в нормальные знначения
        cc.height = ccHeightHorm;
        cc.center = ccCenterNorm;
    }

    /// <summary>
    /// Метод реализации столкновения игрока с препятсвием
    /// </summary>
    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Wall"))
        {
            animator.SetTrigger("Death");
            SetNormalCollaider();
            PlayerSpeed = 0f;
        }
    }
}
