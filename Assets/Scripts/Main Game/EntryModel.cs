using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class EntryModel {
    public string name;//название SCP
    public int code;//его порядковый номер
    public string type;//тип, например эвклид
    public string description;//описание исследованного объекта
    public string procedures;//условия содержания
    public int scpcategory;//категория сцп для отображения текста и картинки при нахождении
    public int randscpcat;//переменная для запоминания текста описания при нахождении
    public double grabcoef;//необходимое кол-во людей для захвата на базу
    public int probesc;//вероятность побега
    public double addsci;//добавление науки после исследования
    public double addinf;//добавление влияния после исследования
    public static EntryModel LoadFromFile(string filename) {
        var text = File.ReadAllText(EntryManager.AppendedSubPath(filename), System.Text.Encoding.UTF8);
        return JsonUtility.FromJson<EntryModel>(text);
    }
}
