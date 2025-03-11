import * as React from "react";
import Button from "@mui/material/Button";
import Stack from "@mui/material/Stack";
import Box from "@mui/material/Box";
import Typography from "@mui/material/Typography";
import Container from "@mui/material/Container";
import Link from "@mui/material/Link";
import Application from "./Application";

export default function Public() {
  return (
    <Box
      sx={{
        bgcolor: "background.paper",
        pt: 8,
        pb: 6,
      }}
    >
      <Container maxWidth="sm">
        <Typography
          component="h1"
          variant="h2"
          align="center"
          color="text.primary"
          gutterBottom
        >
          !This is practicode 3
        </Typography>
        <Typography
          variant="h5"
          align="center"
          color="text.secondary"
          paragraph
        >
          !Created by me
          <br />

        </Typography>
    
        {/* <Application></Application> */}
        <Stack
          sx={{ pt: 4 }}
          direction="row"
          spacing={2}
          justifyContent="center"
        >
        </Stack>
      </Container>
    </Box>
  );
}
