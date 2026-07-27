//=========================================
//描述：随机类工具. 
//作者： Noger 
//创建时间： 2018/09/12 05:02:21  
//版本：v1.0 
//=========================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NOGER {

    public class VixUtils_Random
    {

        public static List<T> RandomsT<T>(List<T> lst) where T : Object
        {
            int count = lst.Count;
            for (int i = 0; i < count - 1; i++)
            {
                int index = Random.Range(0, count);
                T temp = lst[index];
                lst[index] = lst[i];
                lst[i] = temp;
            }
            return lst;
        }

        public static List<Color> Randoms(List<Color> lst) //where T : Object
        {
            int count = lst.Count;
            for (int i = 0; i < count - 1; i++)
            {
                int index = Random.Range(0, count);
                Color temp = lst[index];
                lst[index] = lst[i];
                lst[i] = temp;
            }
            return lst;
        }

        public static List<Vector3> Randoms(List<Vector3> lst)
        {
            int count = lst.Count;
            for (int i = 0; i < count - 1; i++)
            {
                int index = Random.Range(0, count);
                Vector3 temp = lst[index];
                lst[index] = lst[i];
                lst[i] = temp;
            }
            return lst;
        }

        public static List<int> Randoms(List<int> lst)
        {
            int count = lst.Count;
            for (int i = 0; i < count - 1; i++)
            {
                int index = Random.Range(0, count);
                int temp = lst[index];
                lst[index] = lst[i];
                lst[i] = temp;
            }
            return lst;
        }

        public static List<float> Randoms(List<float> lst)
        {
            int count = lst.Count;
            for (int i = 0; i < count - 1; i++)
            {
                int index = Random.Range(0, count);
                float temp = lst[index];
                lst[index] = lst[i];
                lst[i] = temp;
            }
            return lst;
        }



        public static T RandomT<T>(List<T> lst) where T : Object
        {
            int count = lst.Count;
            int index = Random.Range(0, count);

            return lst[index];
        }


        public static Color RandomT(List<Color> lst)
        {
            int count = lst.Count;
            int index = Random.Range(0, count);
            return lst[index];
        }



    }
}



