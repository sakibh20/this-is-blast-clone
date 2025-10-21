using System.Collections.Generic;
using UnityEngine;

public class DocManager : MonoBehaviour
{
    [SerializeField] private List<Doc> allDocs = new List<Doc>();

    public List<Doc> AllDocs => allDocs;
    
    public Doc GetTargetDoc()
    {
        for (int i = 0; i < allDocs.Count; i++)
        {
            Doc doc = allDocs[i];
            if (!doc.IsReserved)
            {
                return doc;
            }
        }

        //Debug.LogWarning("No available Docs found!");
        return null;
    }
    
    public void ReleaseDoc(Doc doc)
    {
        if (doc != null)
            doc.ReleaseDoc();
    }
    
    public void ResetAllDocs()
    {
        foreach (Doc d in allDocs)
            d.ReleaseDoc();
    }
}