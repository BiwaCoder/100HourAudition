from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from app.api.chat import router as chat_router
from app.api.photo import router as photo_router
from app.api.realtime import router as realtime_router
from app.api.live import router as live_router
from app.api.live_title_tour import router as live_title_tour_router
from app.api.live_finale import router as live_finale_router

app = FastAPI(title="100HourAIGame PythonAPI")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)

app.include_router(chat_router)
app.include_router(photo_router)
app.include_router(realtime_router)
app.include_router(live_router)
app.include_router(live_title_tour_router)
app.include_router(live_finale_router)


@app.get("/health")
def health():
    return {"status": "ok"}
