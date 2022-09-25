using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

public class MonsterActivations
{
    public string dataName;
    public int id;
    public int collection;
    public ObservableCollection<MonsterActivationItem> activations = new ObservableCollection<MonsterActivationItem>();
}