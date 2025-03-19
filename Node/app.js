const express = require('express');
const renderApi = require('@api/render-api');

const app = express();
const PORT = process.env.PORT || 3000;

// Auth with your Render API key
renderApi.auth('rnd_rXM3J09WAwq4SoHsVet4iJeYQHtF');

// Endpoint לקבלת רשימת האפליקציות
app.get('/apps', async (req, res) => {
    try {
        const { data } = await renderApi.listServices({ includePreviews: 'true', limit: '20' });
        res.json(data);
    } catch (error) {
        console.error(error);
        res.status(500).json({ error: 'Error fetching apps' });
    }
});

app.listen(PORT, () => {
    console.log(`Server is running on port ${PORT}`);
});
