using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;
using System.IO;
//using System.txt;
//тестовые изменения для  гитхаб


public class BuildingsGrid : MonoBehaviour
{
    public Vector2Int GridSize = new Vector2Int(25, 25); //размер сетки. думаю 20 20 или 30 30 надо сделать

    private Building[,] grid; // массив занятых зданиями ячеек. тут как-то стороны надо будет учитывать!!!и другие характеристики
    private Building flyingBuilding;  //летающее за мышью строение;
    private Camera mainCamera;
    public  static int buildingHeigt = 300; //статическая, потому что оказывается у этой решетки масса экземпляров и разные методы видят разную высоту
    [SerializeField] private Text hWall;
    [SerializeField]  public  Slider wallSlider;



    private void Start() {
        //  Debug.WriteLine();


        try
        {
            wallSlider.onValueChanged.AddListener(delegate { SetBuildingHeightBySlider(); }); //включили слушатель ползунка

            hWall.text = "" + buildingHeigt.ToString();    //установлили строительную высоту начальную в индикатор
        }
        catch (Exception e) { }
    }

    private void Awake()
    {
        grid = new Building[GridSize.x, GridSize.y];
        
        mainCamera = Camera.main; //как ее закрепить в нужном???
    }


    public void SetBuildingHeightBySlider()
    {
        
        buildingHeigt = (int) wallSlider.value *20;  //получили высоту со слайдера умножив на 10 см
        hWall.text = "" + buildingHeigt.ToString(); // забили в текст новую высоту стены
        //Debug.Log(buildingHeigt); //залогировали

    }

    public void SetBuildingHeightBySlider(float h)
    {

        buildingHeigt = (int)h * 20;  //получили высоту со слайдера умножив на 10 см
        hWall.text = "" + buildingHeigt.ToString(); // забили в текст новую высоту стены
        //Debug.Log(buildingHeigt); //залогировали

    }

    
    public void StartPlacingBuilding(String name) //установка летающего здания на новое место. Вариант получения стринг-названия (иначе не компилируется с ресурсами)
    {

        Building pref;   //создали рабочий префаб, пока назначили заданный, потом неясно, что с ним делать.
                                          //    pref =  Resources.Load <Building>("Frame3v4h420");

        // buildingHeigt = 420;
        // Debug.Log(buildingHeigt);


        //String prefabName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(buildingPrefab));  //- раньше вытаскимвали из префаба, теперь наверное обойдемся без этого.
        //Debug.Log(prefabName);

       // if (buildingHeigt != 300)
          //если 420 высота, то переназначаем префаб (потом придется с этим ебаться)

            // Debug.Log("" + prefabName + "h" + buildingHeigt);

            pref = Resources.Load<Building>("" + name + "h" + buildingHeigt + " Variant");


            // Debug.Log(buildingPrefab.getName); 


            //pref = Resources.Load<Building>(buildingPrefab"h420"); // НЕ РАБОТАЕТ!!!!!!

        



        if (flyingBuilding != null)
        {
            Destroy(flyingBuilding.gameObject);  //удаление предыдущего, если он есть
        }

        flyingBuilding = Instantiate(pref); //установка. только совершенно непонятно куда. видимо в префабе все данные?
    }


    private void Update()
    {

        //  hWall.text = buildingHeigt.ToString();

        if (flyingBuilding != null)
        {
           

            var groundPlane = new Plane(Vector3.up, Vector3.zero); //земля - плоскость по нормали и нулевой точке
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (groundPlane.Raycast(ray, out float position))    //определенная нами бесконечная плоскость пересекается лучом, что такое аут флоат позишн?
            {
                Vector3 worldPosition = ray.GetPoint(position);  //вытаскиваем из позиции координаты в мировых координатах

                int x = Mathf.RoundToInt(worldPosition.x);      //округление до целых единиц (метров, кубов?)
                int y = Mathf.RoundToInt(worldPosition.z);      //обрати внимание - переход с z на у

                bool available = true;   //проверка на границы сетки

                if (x < 0 || x > GridSize.x - flyingBuilding.Size.x) available = false; // по координате х нашей сетки
                if (y < 0 || y > GridSize.y - flyingBuilding.Size.y) available = false; //по координате у нашей сетки

                if (available && IsPlaceTaken(x, y)) available = false;  //если можно, но занято другими, значит нельзя

                flyingBuilding.transform.position = new Vector3(x, 0, y);  //устанавливаем на карту???
                flyingBuilding.SetTransparent(available);   // ставим цвет в зависимости от доступности

                if (available && Input.GetMouseButtonDown(0))
                {
                    PlaceFlyingBuilding(x, y);  //пока тут другое было!!!!
                }

            //    hWall.text = buildingHeigt.ToString();
            }
        }
        else { 
        }
    }

    private bool IsPlaceTaken(int placeX, int placeY)   //можно ли поставить здание в указанное место?
    {
        for (int x = 0; x < flyingBuilding.Size.x; x++)   
        {
            for (int y = 0; y < flyingBuilding.Size.y; y++)
            {
                if (grid[placeX + x, placeY + y] != null) return true;  //если в учетной решетке не пусто - учитывается какое-то здание, то клетка занята
            }
        }

        return false;
    }

    private void PlaceFlyingBuilding(int placeX, int placeY)   //метод, ставящий на карту летающее здание, принимаем есто, куда оно ставится
    {
        for (int x = 0; x < flyingBuilding.Size.x; x++)  //идем по всему размеру здания - здесь по х
        {
            for (int y = 0; y < flyingBuilding.Size.y; y++)  //здесь по у
            {
                grid[placeX + x, placeY + y] = flyingBuilding;  //присваеваем ячейке учетной таблицы данные здания, возможно надо давать нечто более объемное - для сшивки!
            }
        }
        
        flyingBuilding.SetNormal(); //устанавливаем нормальный цвет
        flyingBuilding = null;  //отцепляем от имеющегося летающего здания.
    }
}
