using UnityEngine;
using System.Collections.Generic;

public class Water : MonoBehaviour, ISoundEventGenerator
{
	public int waveResolution = 5;
	public float waveFactor = 0.007f;
	public float maxWave = 1.5f;
	public float bubblesPerSquareUnit = 5f;
	public bool showOutlineGizmo = true;
    public float ambientWaveMagnitute = 0.03f;
    public float ambientWaveFrequency = 0.05f;
    private float ambientWaveTimer = 0;
	
	private float[] xpositions;
	private float[] ypositions;
	private float[] velocities;
	private float[] accelerations;
	
	private GameObject[] meshObjects;
	private GameObject[] colliders;
	private Mesh[] meshes;
	
	private const float springConstant = 0.02f;
	private const float damping = 0.04f;
	private const float spread = 0.05f;
	
	private Vector2 origin;
	private Vector2 size;
	private float surfaceHeight;

	private float[] leftDeltas;
	private float[] rightDeltas;
	private Vector3[] meshVerts = new Vector3[4];
    
    private SoundEvent playSplashSound;
    public event SoundEvent OnSoundEvent
    {
        add { playSplashSound += value; }
        remove { playSplashSound -= value; }
    }
	
	[System.Serializable]
	public class SplashEffect
	{
		public GameObject effect;
		public float timeout = 0.5f;
		public int particleCount = 20;
		public int minParticles = 5;
		public float lifetime = 5.0f;
	}
	public SplashEffect splash = new SplashEffect();
	public static Dictionary<GameObject, float> splashTimes = new Dictionary<GameObject,float>();
	
	private void Awake()
	{       
		GetComponent<ParticleSystem>().emissionRate = bubblesPerSquareUnit * transform.localScale.x * transform.localScale.y;

		origin = transform.position - transform.lossyScale / 2;
		surfaceHeight = transform.position.y + transform.lossyScale.y / 2;
		size = transform.lossyScale;
		
		int edgeCount = Mathf.RoundToInt(size.x) * waveResolution;
		int nodeCount = edgeCount + 1;

		xpositions = new float[nodeCount];
		ypositions = new float[nodeCount];
		velocities = new float[nodeCount];
		accelerations = new float[nodeCount];
		leftDeltas = new float[nodeCount];
		rightDeltas = new float[nodeCount];
		
		meshObjects = new GameObject[edgeCount];
		meshes = new Mesh[edgeCount];
		colliders = new GameObject[edgeCount];
		
		// Init surface nodes
		for (int i = 0; i < nodeCount; i++)
		{
			ypositions[i] = surfaceHeight;
			xpositions[i] = origin.x + size.x * i / edgeCount;
			accelerations[i] = 0;
			velocities[i] = 0;
		}
		
		// Create meshes
		for (int i = 0; i < edgeCount; i++)
		{
			meshes[i] = new Mesh();
			
			Vector3[] Vertices = new Vector3[4];
			Vertices[0] = new Vector3(xpositions[i], ypositions[i], 0);
			Vertices[1] = new Vector3(xpositions[i + 1], ypositions[i + 1], 0);
			Vertices[2] = new Vector3(xpositions[i], origin.y, 0);
			Vertices[3] = new Vector3(xpositions[i + 1], origin.y, 0);
			
			Vector2[] UVs = new Vector2[4];
			UVs[0] = new Vector2(0, 1);
			UVs[1] = new Vector2(1, 1);
			UVs[2] = new Vector2(0, 0);
			UVs[3] = new Vector2(1, 0);
			
			int[] tris = new int[6] { 0, 1, 3, 3, 2, 0 };
			
			meshes[i].vertices = Vertices;
			meshes[i].uv = UVs;
			meshes[i].triangles = tris;
			
			// Instantiate mesh as child of water object
			meshObjects[i] = (GameObject)Instantiate(Resources.Load("WaterMesh"), Vector3.zero, Quaternion.identity);
			meshObjects[i].GetComponent<MeshFilter>().mesh = meshes[i];
			meshObjects[i].transform.parent = transform;
			meshObjects[i].GetComponent<MeshRenderer>().sortingLayerName = "Water";
			
			// Create colliders
			colliders[i] = new GameObject();
			colliders[i].name = "Trigger";
			colliders[i].layer = LayerMask.NameToLayer("Water");
			colliders[i].AddComponent<BoxCollider2D>();
			colliders[i].transform.position = new Vector3(origin.x + size.x * (i + 0.5f) / edgeCount, surfaceHeight, 0);
			colliders[i].transform.localScale = new Vector3(size.x / edgeCount, 0.5f, 1);
			colliders[i].transform.parent = transform;
			colliders[i].GetComponent<BoxCollider2D>().isTrigger = true;
			colliders[i].AddComponent<WaterDetector>();
		}
	}
	
	public void Splash(GameObject obj, float x, float force)
	{
		float velocity = Mathf.Clamp(force * waveFactor, -maxWave, maxWave);

		if (x >= xpositions[0] && x <= xpositions[xpositions.Length-1])
		{
			// Add velocity of colliding to spring in water nodes
			float xOffset = x - xpositions[0];
			int index = Mathf.RoundToInt((xpositions.Length - 1) * (xOffset / (xpositions[xpositions.Length - 1] - xpositions[0])));
			velocities[index] += velocity;
			
			// Splash
			if (splashTimes.ContainsKey(obj))
			{
				if (Time.time > splashTimes[obj] + splash.timeout)
				{
					splashTimes[obj] = Time.time;
					GenerateSplashEffect(obj.transform.position.x, -velocity);
				}
			}
			else
			{
				splashTimes[obj] = Time.time;
				GenerateSplashEffect(obj.transform.position.x, -velocity);
			}
		}
	}
	
	private void GenerateSplashEffect(float x, float velocity)
	{
		int numParticles = (int)(Mathf.Abs(velocity) * splash.particleCount);
		if(numParticles < splash.minParticles)
			numParticles = splash.minParticles;
		
		Vector3 position = new Vector3(x, transform.position.y + transform.lossyScale.y / 2, -5);
		GameObject effect = (GameObject)Instantiate(splash.effect, position, Quaternion.identity);
		effect.GetComponentInChildren<ParticleSystem>().maxParticles = numParticles;
		Destroy(effect, splash.lifetime);

		// Play sound effect
		if (playSplashSound != null)
		{
			float volume = Mathf.Clamp(Mathf.Abs(velocity)*10, 0.2f, 1.0f);
			playSplashSound(volume);
		}
	}

	private void Update()
    {
        ambientWaveTimer += Time.deltaTime;
        if (ambientWaveTimer >= ambientWaveFrequency)
        {
            ambientWaveTimer = 0;
            int randomIndex = Random.Range(0, velocities.Length-1);
            velocities[randomIndex] -= ambientWaveMagnitute;
        }
    }
	
	private void FixedUpdate()
	{
		// Integrate spring kinetic variables
		for (int i = 0; i < xpositions.Length; i++)
		{
			float force = springConstant * (ypositions[i] - surfaceHeight) + velocities[i] * damping;
			accelerations[i] = -force;
			ypositions[i] += velocities[i];
			velocities[i] += accelerations[i];
			leftDeltas[i] = 0;
			rightDeltas[i] = 0;
		}
		
		// Propogate wave effects
		for (int j = 0; j < 8; j++)
		{
			for (int i = 0; i < xpositions.Length; i++)
			{
				if (i > 0)
				{
					leftDeltas[i] = spread * (ypositions[i] - ypositions[i-1]);
					velocities[i - 1] += leftDeltas[i];
				}
				if (i < xpositions.Length - 1)
				{
					rightDeltas[i] = spread * (ypositions[i] - ypositions[i + 1]);
					velocities[i + 1] += rightDeltas[i];
				}
			}
			
			for (int i = 0; i < xpositions.Length; i++)
			{
				if (i > 0)
					ypositions[i-1] += leftDeltas[i];
				if (i < xpositions.Length - 1)
					ypositions[i + 1] += rightDeltas[i];
			}
		}

		// Update meshes
		for (int i = 0; i < meshes.Length; i++)
		{
			meshVerts[0] = new Vector3(xpositions[i], ypositions[i], 0);
			meshVerts[1] = new Vector3(xpositions[i + 1], ypositions[i + 1], 0);
			meshVerts[2] = new Vector3(xpositions[i], origin.y, 0);
			meshVerts[3] = new Vector3(xpositions[i + 1], origin.y, 0);
			meshes[i].vertices = meshVerts;
		}
	}
	
	private void OnDrawGizmos()
	{
		if (showOutlineGizmo)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawWireCube(transform.position, transform.lossyScale);
		}
	}
}
