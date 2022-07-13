using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZonesResourcesManager : MonoBehaviour
{
    /// <summary>
    /// List of all players bases
    /// </summary>
    private List<BaseModel> basesPool = new List<BaseModel>();

    /// <summary>
    /// Get list of all players bases
    /// </summary>
    /// <returns></returns>
    public List<BaseModel> GetAllBases()
    {
        return basesPool;
    }

    /// <summary>
    /// Add new base by name
    /// </summary>
    /// <param name="name"></param>
    public void AddBase(string name)
    {
        BaseModel baseModel= new BaseModel();
        baseModel.name=name;
        basesPool.Add(baseModel);
    }

    /// <summary>
    /// Add new base with all data
    /// </summary>
    /// <param name="baseModel"></param>
    public void AddBase(BaseModel baseModel)
    {
        basesPool.Add(baseModel);
    }

    /// <summary>
    /// Update base
    /// </summary>
    /// <param name="model"></param>
    public void UpdateBase(BaseModel model)
    {
        if(basesPool.Find(x => x.name == model.name)!=null)
        {
            int index=basesPool.IndexOf(basesPool.Find(x => x.name == model.name));
            basesPool[index]=model;
        }
    }

    /// <summary>
    /// Find base by name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public BaseModel GetBase(string name)
    {
        return basesPool.Find(x => x.name==name);
    }
}
