using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZonesResourcesManager : MonoBehaviour
{
    private List<BaseModel> basesPool = new List<BaseModel>();
    public void AddBase(string name)
    {
        BaseModel baseModel= new BaseModel();
        baseModel.name=name;
        basesPool.Add(baseModel);
    }
    public void AddBase(BaseModel baseModel)
    {
        basesPool.Add(baseModel);
    }
    public void UpdateBase(BaseModel model)
    {
        if(basesPool.Find(x => x.name == model.name)!=null)
        {
            int index=basesPool.IndexOf(basesPool.Find(x => x.name == model.name));
            basesPool[index]=model;
        }
    }
    public BaseModel GetBase(string name)
    {
        return basesPool.Find(x => x.name==name);
    }
}
