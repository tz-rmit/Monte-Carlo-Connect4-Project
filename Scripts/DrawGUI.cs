using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class DrawGUI : MonoBehaviour
{
	// These references are already set up in the "FullGame" scene.
	// You can use them to show/hide the game over panel and update the game over
	// text depending on whether the player won or lost.
	public GameObject GameOverPanel;
	public TMPro.TextMeshProUGUI GameOverText;

	private bool _isGameOver = false;
	private float _defaultTimeScale;

	public Sprite HeartSprite;
	public Sprite FlySprite;

	private int _iconSize = 20;
	private int _iconSeparation = 10;

	private Texture2D _heartTex;
	private Texture2D _flyTex;

	private Frog _frogScript;

	void Start()
	{
		_heartTex = SpriteToTexture(HeartSprite);
		_flyTex = SpriteToTexture(FlySprite);

		_defaultTimeScale = Time.timeScale;
		_frogScript = GameObject.Find("Frog").GetComponent<Frog>();
	}

	public void HandleGameOver(bool won)
	{
		_isGameOver = true;
		Time.timeScale = 0.0f;

		if (won)
        {
			GameOverText.text = "You won!";
		}
        else
        {
			GameOverText.text = "You died!";
		}
		
		StartCoroutine(GameOverSequence());
	}

	void Update()
	{
		// If game is over
		if (_isGameOver)
		{
			// If R is pressed, restart the current scene
			if (Input.GetKeyDown(KeyCode.R))
			{
				SceneManager.LoadScene(SceneManager.GetActiveScene().name);
				Time.timeScale = _defaultTimeScale;
			}
		}
	}

	void OnGUI()
	{
		GUI.Box(new Rect(10, 10, 30 * _frogScript.MaxFlies + 10, 60), "");

		for (int i = 0; i < _frogScript.Health; i++)
		{
			GUI.DrawTexture(new Rect(20 + (_iconSize + _iconSeparation) * i, 20, _iconSize, _iconSize), _heartTex, ScaleMode.ScaleToFit, true, 0.0f);
		}

		for (int i = 0; i < _frogScript.FliesCaught; i++)
		{
			GUI.DrawTexture(new Rect(20 + (_iconSize + _iconSeparation) * i, 45, _iconSize, _iconSize), _flyTex, ScaleMode.ScaleToFit, true, 0.0f);
		}
	}

	// Helper function to convert sprites to textures.
	// Follows the code from http://answers.unity3d.com/questions/651984/convert-sprite-image-to-texture.html
	private Texture2D SpriteToTexture(Sprite sprite)
	{
		if (sprite.rect.width != sprite.texture.width)
		{
			Texture2D texture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
			Color[] pixels = sprite.texture.GetPixels((int)sprite.textureRect.x, (int)sprite.textureRect.y, (int)sprite.textureRect.width, (int)sprite.textureRect.height);
			texture.SetPixels(pixels);
			texture.Apply();

			return texture;
		}
		else
		{
			return sprite.texture;
		}
	}

	private IEnumerator GameOverSequence()
	{
		GameOverPanel.SetActive(true);
		yield return new WaitForSeconds(5.0f);
	}
}
