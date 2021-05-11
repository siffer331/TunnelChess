using Godot;
using System;
using System.Collections.Generic;

public class BoardUI : Node2D
{
	[Export]
	public Color black = new Color("#41485e");
	[Export]
	private Color white = new Color("#a8a8be");
	[Export]
	private Color selected = new Color("#c8b051");
	[Export]
	private Color avaliable = new Color("#d0955b");
	[Export]
	private Color enemy = new Color("#d94141");
	[Export]
	private int height = 430;
	[Export]
	private int space = 60;
	[Export]
	private Color defaultUI = new Color("#323250");
	[Export]
	private Color selectedUI = new Color("#262535");
	
	//private AI ai;
	private float aiMove = 100;
	private int selectedTile = -1;
	private Board board = new Board();
	private int promotion = Pieces.Queen;
	private Stack<BoardMove> stack = new Stack<BoardMove>();
	private const String infoPath = "../Canvas/Info/Margin/Grid/";
	
	//test strings:
	//  rnbqkbnr/2pppppp/8/pP6/8/8/1PPPPPPP/RNBQKBNR w
	//  rnbqkbnr/p1pppppp/8/1P6/8/8/1PPPPPPP/RNBQKBNR b
	//  rnbqkbnr/p1pppppp/8/1p6/P7/8/1PPPPPPP/RNBQKBNR w
	//  rnbqkbnr/pppppppp/8/8/P7/8/1PPPPPPP/RNBQKBNR b
	//  rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w
	//  rnbq1k1r/pp1Pbppp/2p5/3B4/8/8/PPP1NnPP/RNBQK2R b
	List<BoardMove> points = new List<BoardMove>();
	List<int> highlights = new List<int>();
	
	//Draw board with colors
	public override void _Draw() {
		for(int b = 0; b < 2; b++) {
			for(int i = 0; i < 8; i++) {
				for(int j = 0; j < 8; j++) {
					int tile = i+j*8+64*b;
					Color c = (i+j)%2 == 0 ? black: white;
					Color overlay = c;
					//if(board.IsThreatened(tile, board.turn)) overlay = new Color(1,0,0);
					BoardMove m = PointsHas(tile);
					foreach(int highlight in highlights) {
						if(highlight == tile) {
							overlay = new Color(0,1,0);
							break;
						}
					}
					if(stack.Count>0&&(tile==stack.Peek().toB||tile==stack.Peek().fromB))
						overlay = selected;
					if(m.toB != -1) {
						overlay = avaliable;
						if(board.board[tile] != 0) overlay = enemy;
					}
					if(tile == selectedTile) overlay = selected;
					//if(board.pieces[0].ContainsKey(tile)) overlay = new Color(1,1,1);
					//if(board.pieces[1].ContainsKey(tile)) overlay = new Color(0,0,0); 
					c = c.LinearInterpolate(overlay, .75f);
					Vector2 pos = new Vector2(i+8*b,j)*height/8;
					pos.x += space*b;
					Rect2 rect = new Rect2(pos, Vector2.One*height/8);
					DrawRect(rect, c);
				}
			}
		}
	}
	
	public BoardMove PointsHas(int to) {
		foreach(BoardMove move in points) {
			if(to == move.toB) return move;
		}
		return new BoardMove(-1,-1,-1,-1,-1);
	}
	
	//make sprites
	public override void _Ready() {
		for(int b = 0; b < 2; b++) {
			for(int i = 0; i < 8; i++) {
				for(int j = 0; j < 8; j++) {
					PackedScene scene = ResourceLoader.Load("res://SRC/Game/Piece.tscn") as PackedScene;
					Sprite sprite = scene.Instance() as Sprite;
					sprite.Name = (i+8*j+64*b).ToString();
					sprite.Position = new Vector2(i+.5f+8*b,j+.5f)*height/8 + new Vector2(space*b,0);
					sprite.Scale = Vector2.One*height*.8f/2664;
					AddChild(sprite);
				}
			}
		}
		//ai = new AI(board);
		//for(int i = 1; i < 5; i++) GD.Print(i, " ",ai.CountMoves(i));
		DrawBoard(board);
		_on_Button_pressed(Pieces.Queen);
	}
	/*
	public override void _Process(float delta) {
		if(aiMove < 90 && aiMove > 0.1) {
			BoardMove move = ai.GetBestMove(5);
			stack.Push(move);
			board.MakeMove(move);
			DrawBoard(board);
			Update();
			aiMove = 100;
		}
		else aiMove += delta;
	}*/
	
	//visualise pieces
	public void DrawBoard(Board toDraw) {
		for(int i = 0; i < 128; i++) {
			(GetNode(i.ToString()) as Sprite).Frame = toDraw.board[i];
		}
	}
	
	//get mouse click
	public override void _Input(InputEvent @event) {
		if(@event.IsActionPressed("click")) {
			Vector2 mouse = GetLocalMousePosition();
			int tile = 0;
			if(mouse.x >= height+space) {
				mouse.x -= height+space;
				tile = 64;
			}
			if(Math.Min(mouse.x, mouse.y) >= 0 && Math.Max(mouse.x, mouse.y) <= height) {
				tile += (int)(
					Math.Floor(mouse.x*8/height) +
					Math.Floor(mouse.y*8/height)*8
				);
				BoardMove move = PointsHas(tile);
				if(move.toB == -1) {
					points = board.GetMoves(tile);
					selectedTile = tile;
					Update();
				}
				else {
					if(move.promotion != -1) move.promotion = promotion;
					stack.Push(move);
					board.MakeMove(move);
					points = new List<BoardMove>();
					selectedTile = -1;
					DrawBoard(board);
					Update();
					aiMove = 00;
				}
			}
		}
		if(@event.IsActionPressed("undo")) {
			if(stack.Count > 0) {
				board.UndoMove(stack.Pop());
				DrawBoard(board);
				Update();
			}
		}
	}
	
	private void _on_Button_pressed(int piece)
	{
		Panel panel = GetNode("../Canvas/UI/Margin/Grid/" + promotion.ToString() + "/Panel") as Panel;
		panel.Modulate = defaultUI;
		promotion = piece;
		panel = GetNode("../Canvas/UI/Margin/Grid/" + promotion.ToString() + "/Panel") as Panel;
		panel.Modulate = selectedUI;
	}
}
