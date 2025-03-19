import axios from "axios";
import { jwtDecode } from "jwt-decode";


axios.defaults.baseURL = process.env.REACT_APP_API_URL
console.log('process.env.API_URL', process.env.REACT_APP_API_URL)
setAuthorizationBearer();

function saveAccessToken(authResult) {
  localStorage.setItem("access_token", authResult.token);
  setAuthorizationBearer();
}

function setAuthorizationBearer() {
  const accessToken = localStorage.getItem("access_token");
  console.log(accessToken, "access_token");

  if (accessToken) {
    axios.defaults.headers.common["Authorization"] = `Bearer ${accessToken}`;
  }
}

axios.interceptors.response.use(
  function (response) {
    return response;
  },
  function (error) {
    console.log(error, "err");

    if (error.response.status === 401) {
      return (window.location.href = "/login");
    }
    if (error.response.status === 404) {
      return (window.location.href = "/register");
    }
    else {
      return Promise.reject(error);

    }
  }
);

export default {
  getLoginUser: () => {
    const accessToken = localStorage.getItem("access_token");
    if (accessToken) {
      return jwtDecode(accessToken);
    }
    return null;
  },

  logout: () => {
    localStorage.setItem("access_token", "");
  },

  register: async (email, password) => {
    const res = await axios.post("/register", { email, password });
    saveAccessToken(res.data);
  },

  login: async (email, password) => {
    const res = await axios.post("/login", { email, password });
    // console.log("login",res);

    // if (res.response.status == 404) {
    //   return (window.location.href = "/register");

    // }
    // else {
    saveAccessToken(res.data);

    // }
  },

  getPublic: async () => {
    const res = await axios.get("/public");
    return res.data;
  },

  getPrivate: async () => {
    const res = await axios.get("/private");
    return res.data;
  },

  getTasks: async () => {
    try {

      const res = await axios.get("/getTasks")
      if (res.status == 200)
        return res.data;
      else
        console.log("getTasks", res.data);

    } catch (error) {
      console.error("Error in getTask:", error);

    }
  },

  addTask: async (name) => {
    console.log('addTask', name)
    //TODO
    try {
      const res = await axios.post("/addItem", { name })
      if (res.status == 200)
        return res.data;
      else
        console.log("addTask", res.data);
    } catch (error) {
      console.error("Error in addTask:", error);

    }

  },

  setCompleted: async (id, isComplete) => {
    console.log('setCompleted', { id, isComplete });
    try {
      const res = await axios.put(`${id}?isComplete=${isComplete}`);
      if (res.status === 200) {
        return res.data;
      } else {
        console.log("setComplete", res.data);
      }
    } catch (error) {
      console.error("Error in setCompleted:", error);
    }
  },

  deleteTask: async (id) => {
    console.log('deleteTask')
    try {
      const res = await axios.delete(`${id}`)
      if (res.status == 200)
        return res.data;
      else
        console.log("deleteTask", res.data);
    } catch (error) {
      console.error("Error in setCompleted:", error);

    }

  }


};
