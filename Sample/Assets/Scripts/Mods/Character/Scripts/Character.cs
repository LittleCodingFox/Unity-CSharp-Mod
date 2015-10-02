using UnityEngine;
using System.Collections;

public class Character : Moddable
{
	public SpriteRenderer TheSpriteRenderer;
    public Sprite TheSprite;
    public const float Speed = 10;
	
	void Start()
	{
        TheSpriteRenderer = gameObject.AddComponent<SpriteRenderer>();

#if MODMANAGER_LOADS_FROM_MEMORY
        TheSprite = Resources.Load<Sprite>("Textures/Sprite");

        TheSpriteRenderer.sprite = TheSprite;
#else
        StartCoroutine(LoadSprite());
#endif
	}

    void Update()
    {
        if(Input.GetKey(KeyCode.LeftArrow))
        {
            transform.position = new Vector3(transform.position.x - Speed * Time.deltaTime, transform.position.y, transform.position.z);
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.position = new Vector3(transform.position.x + Speed * Time.deltaTime, transform.position.y, transform.position.z);
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + Speed * Time.deltaTime, transform.position.z);
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - Speed * Time.deltaTime, transform.position.z);
        }
    }

    IEnumerator LoadSprite()
    {
        WWW TheSpriteWWW = new WWW("file://" + TheMod.Path.Replace('\\', '/') + "/Textures/Sprite.png");

        yield return TheSpriteWWW;

        if(TheSpriteWWW.error != null)
        {
            Debug.Log("Failed to load sprite '" + TheMod.Path + "/Textures/Sprite.png" + "': " + TheSpriteWWW.error);
        }
        else
        {
            TheSprite = Sprite.Create(TheSpriteWWW.texture, new Rect(Vector2.zero, new Vector2(TheSpriteWWW.texture.width, TheSpriteWWW.texture.height)), new Vector2(0.5f, 0.5f));

            if(TheSprite != null)
            {
                TheSpriteRenderer.sprite = TheSprite;
            }
        }
    }
}