

import axios from "axios";

axios.defaults.baseURL = "http://localhost:5067/api";

// טיפול בשגיאות
axios.interceptors.response.use(
  response => response,
  error => {
    console.error("API Error:", error);
    return Promise.reject(error);
  }
);

export default {
  // שליפת כל המשימות
  getTasks: async () => {
    const result = await axios.get("/items");
    return result.data;
  },

  // הוספת משימה חדשה
  addTask: async (name) => {
    const newTask = {
      name: name,
      isComplete: false
    };
    const result = await axios.post("/items", newTask);
    return result.data;
  },

  // עדכון משימה (סימון כבוצע)
  setCompleted: async (id, task) => {
    const updatedTask = {
      id: id,
      name: task.name, // שומרים על השם המקורי!
      isComplete: !task.isComplete
    };
    const result = await axios.put(`/items/${id}`, updatedTask);
    return result.data;
  },

  // מחיקת משימה
  deleteTask: async (id) => {
    const result = await axios.delete(`/items/${id}`);
    return result.data;
  }
};
