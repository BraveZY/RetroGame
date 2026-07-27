using UnityEngine;
using System.Collections;

public abstract class AtBehaviour : MonoBehaviour
{
	Transform _Transform;
	Renderer _Renderer;
	Rigidbody _Rigidbody;
	Rigidbody2D _Rigidbody2D;
	BoxCollider2D _BoxCollider2D;
	CircleCollider2D _CircleCollider2D;

	MeshRenderer _meshRenderer;

	MeshFilter _meshFilter;

	public MeshFilter cachedMeshFilter
	{
		get
		{
			if (_meshFilter == null) {
				_meshFilter = GetComponent<MeshFilter>();
			}
			return _meshFilter;
		}
	}


	public MeshRenderer cachedMeshRenderer
	{
		get
		{
			if (_meshRenderer == null) {
				_meshRenderer = GetComponent<MeshRenderer>();
			}
			return _meshRenderer;
		}
	}

	public new Transform transform
	{
		get
		{
			if (_Transform == null) {
				_Transform = GetComponent<Transform>();
			}
			return _Transform;
		}
	}

	public new Renderer renderer
	{
		get
		{
			if (_Renderer == null) {
				_Renderer = GetComponent<Renderer>();
			}
			return _Renderer;
		}
	}

	public new Rigidbody rigidbody
	{
		get
		{
			if (_Rigidbody == null) {
				_Rigidbody = GetComponent<Rigidbody>();
			}
			return _Rigidbody;
		}
	}

	public new Rigidbody2D rigidbody2D  
	{
		get
		{
			if (_Rigidbody2D == null) {
				_Rigidbody2D = GetComponent<Rigidbody2D>();
			}
			return _Rigidbody2D;
		}

	}

	public BoxCollider2D boxCollider2D
	{
		get
		{
			if(_BoxCollider2D == null)
			{
				_BoxCollider2D = GetComponent<BoxCollider2D>();
			}
			return _BoxCollider2D;
		}
	}
	public  CircleCollider2D circleCollider2D
	{
		get
		{
			if(_CircleCollider2D == null)
			{
				_CircleCollider2D = GetComponent<CircleCollider2D>();
			}
			return _CircleCollider2D;
		}
	}

	private Collider2D _collider2d = null;
	public new Collider2D collider2D
	{
		get
		{
			if(_collider2d == null)
			{
				_collider2d = GetComponent<Collider2D>();
			}
			return _collider2d;
		}
	}

	private Collider2D[] _collider2ds = null;

	public  Collider2D[] collider2Ds
	{
		get
		{
			if(_collider2ds == null)
			{
				_collider2ds = GetComponents<Collider2D>();
			}
			return _collider2ds;
		}
	}

	private Renderer[] _renderers = null;
	public Renderer[] Renderers
	{
		get
		{
			if(_renderers == null)
			{
				_renderers = GetComponentsInChildren<Renderer>(true);
			}
			return _renderers;
		}
	}
}