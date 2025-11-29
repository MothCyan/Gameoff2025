using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 房间管理器 - 管理场景中的所有房间
/// </summary>
public class RoomManager : MonoBehaviour
{
    [Header("房间列表")]
    [Tooltip("场景中的所有房间")]
    public List<Room> rooms = new List<Room>();
    
    [Header("当前房间")]
    [SerializeField] private Room currentRoom;

    void Start()
    {
        // 自动查找场景中的所有房间
        FindAllRooms();
        
        Debug.Log($"[RoomManager] 找到 {rooms.Count} 个房间");
    }

    /// <summary>
    /// 自动查找场景中的所有房间
    /// </summary>
    private void FindAllRooms()
    {
        Room[] foundRooms = FindObjectsOfType<Room>();
        foreach (var room in foundRooms)
        {
            if (!rooms.Contains(room))
            {
                rooms.Add(room);
            }
        }
    }

    /// <summary>
    /// 设置当前房间
    /// </summary>
    public void SetCurrentRoom(Room room)
    {
        if (currentRoom != room)
        {
            currentRoom = room;
            Debug.Log($"[RoomManager] 当前房间: {room.gameObject.name}");
        }
    }

    /// <summary>
    /// 获取当前房间
    /// </summary>
    public Room GetCurrentRoom()
    {
        return currentRoom;
    }

    /// <summary>
    /// 获取所有已通过的房间数量
    /// </summary>
    public int GetClearedRoomsCount()
    {
        int count = 0;
        foreach (var room in rooms)
        {
            if (room.GetRoomState() == RoomState.Cleared)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// 获取所有房间总数
    /// </summary>
    public int GetTotalRoomsCount()
    {
        return rooms.Count;
    }

    /// <summary>
    /// 检查是否所有房间都已通过
    /// </summary>
    public bool AreAllRoomsCleared()
    {
        foreach (var room in rooms)
        {
            if (room.GetRoomState() != RoomState.Cleared)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 重置所有房间（用于测试）
    /// </summary>
    public void ResetAllRooms()
    {
        foreach (var room in rooms)
        {
            room.ResetRoom();
        }
        Debug.Log("[RoomManager] 所有房间已重置");
    }
}
