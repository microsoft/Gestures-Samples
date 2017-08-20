using UnityEngine;

public class PrimitiveFactory : MonoBehaviour
{
    public void CreateRandomPrimitive()
    {
        var primitives = System.Enum.GetValues(typeof(PrimitiveType)) as PrimitiveType[];

        var rndIndex = Random.Range(0, primitives.Length);
        var primitive = GameObject.CreatePrimitive(primitives[rndIndex]);

        primitive.AddComponent<Rigidbody>().useGravity = false;
        primitive.GetComponent<Rigidbody>().velocity = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        primitive.GetComponent<Renderer>().material.color = Random.ColorHSV();
    }

    public void DestroyAllPrimitives()
    {
        var allPrimitives = FindObjectsOfType<Rigidbody>();

        foreach (var primitive in allPrimitives)
        {
            Destroy(primitive.gameObject);
        }
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CreateRandomPrimitive();
        }
        if (Input.GetMouseButtonDown(1))
        {
            DestroyAllPrimitives();
        }
    }
}