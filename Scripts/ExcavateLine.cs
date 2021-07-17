using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExcavateLine : MonoBehaviour
{
    [SerializeField] private GameObject _bucketObj;
    [SerializeField] private float _lineWidth;
    [SerializeField] private bool _isExcavateLineVisible;

    private Rigidbody _rigidbody;
    private CapsuleCollider _collider;
    private LineRenderer _lineRenderer;
    private TerrainManager _terrainManager;
    private Vector3[] _lineEnd;
    private bool _isDeformable;

#if UNITY_EDITOR
    private DeformTools.TagHelper _tagHelper = new DeformTools.TagHelper();
#endif
    private string _terrainTagName = "Terrain";
    private string _bucketTagName = "Bucket";

    private void Awake()
    {
        this.gameObject.AddComponent<Rigidbody>();
        this.gameObject.AddComponent<CapsuleCollider>();
        if (this._isExcavateLineVisible) this.gameObject.AddComponent<LineRenderer>();

#if UNITY_EDITOR
        // Tag setting
        this._tagHelper.AddTag(this._bucketTagName);
#endif
        this.gameObject.tag = this._bucketTagName;

        this._bucketObj.tag = this._bucketTagName;
        Transform children = this._bucketObj.GetComponentInChildren<Transform>();
        if (children.childCount != 0)
        {
            foreach (Transform child in children)
            {
                child.tag = this._bucketTagName;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        this._rigidbody = this.gameObject.GetComponent<Rigidbody>();
        this._rigidbody.isKinematic = true;
        this._collider = this.gameObject.GetComponent<CapsuleCollider>();
        this._collider.height = this._lineWidth;
        this._collider.isTrigger = true;
        this._collider.radius = 0.01f;
        this._collider.direction = 2;   // Z-axis

        if (this._isExcavateLineVisible)
        {
            this._lineRenderer = this.gameObject.GetComponent<LineRenderer>();
            this._lineRenderer.startWidth = 0.05f;
        }

        this._terrainManager = GameObject.FindGameObjectWithTag(this._terrainTagName).GetComponent<TerrainManager>();
        this._isDeformable = false;
    }

    // Update is called once per frame
    void Update()
    {
        this._lineEnd = new Vector3[] { new Vector3(0, 0, -this._lineWidth / 2), new Vector3(0, 0, this._lineWidth / 2) };
        if (this._isExcavateLineVisible)
        {
            this._lineRenderer.SetPositions(new Vector3[] { this.transform.position + this._lineEnd[0], this.transform.position + this._lineEnd[1] });
        }
        if (this._isDeformable)
        {
            Vector3[] targets = this.GetExcavateArea(this.transform.position + this._lineEnd[0], this.transform.position + this._lineEnd[1]);
            this._terrainManager.ExcavateWithSand(targets);
        }

        Vector3[] line = new Vector3[] { this.transform.position + this._lineEnd[0], this.transform.position + this._lineEnd[1] };
        if (line[0].y - 0.1f > this._terrainManager.FromTerrainPositionY(this._terrainManager.GetHeightmap(line[0])) &&
            line[1].y - 0.1f > this._terrainManager.FromTerrainPositionY(this._terrainManager.GetHeightmap(line[1])))
        {
            this._isDeformable = false;
        }
        else
        {
            this._isDeformable = true;
        }
    }

    private void OnDisable()
    {
        GameObject.Destroy(this.GetComponent<CapsuleCollider>());
    }

    private Vector3[] GetExcavateArea(Vector3 start, Vector3 end)
    {
        List<Vector3> DeformVerts = new List<Vector3>();
        int loopNum = 17;
        for (float i = 0.0f; i <= 1.0f; i += 1.0f / (float)loopNum)
        {
            DeformVerts.Add(i * start + (1 - i) * end);
        }
        return DeformVerts.ToArray();
    }
}
