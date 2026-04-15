-- Card comments system
-- Run this SQL in your Supabase SQL Editor

-- Create card_comments table
CREATE TABLE IF NOT EXISTS card_comments (
    id BIGSERIAL PRIMARY KEY,
    card_id TEXT NOT NULL,
    user_name TEXT NOT NULL,
    comment_text TEXT NOT NULL,
    is_public BOOLEAN DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Create index for faster queries
CREATE INDEX IF NOT EXISTS idx_card_comments_card_id ON card_comments(card_id);
CREATE INDEX IF NOT EXISTS idx_card_comments_public ON card_comments(is_public);

-- Enable Row Level Security
ALTER TABLE card_comments ENABLE ROW LEVEL SECURITY;

-- Policy: Anyone can read public comments
CREATE POLICY "Anyone can view public comments"
    ON card_comments
    FOR SELECT
    USING (is_public = true);

-- Policy: Anyone can submit comments (but they're private by default)
CREATE POLICY "Anyone can submit comments"
    ON card_comments
    FOR INSERT
    WITH CHECK (true);

-- Create a function to update updated_at timestamp
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Create trigger to auto-update updated_at
CREATE TRIGGER update_card_comments_updated_at
    BEFORE UPDATE ON card_comments
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();
