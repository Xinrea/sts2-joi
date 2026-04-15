-- Supabase setup for card voting system
-- Run this SQL in your Supabase SQL Editor

-- Create card_votes table
CREATE TABLE IF NOT EXISTS card_votes (
    id BIGSERIAL PRIMARY KEY,
    card_id TEXT NOT NULL,
    vote_type TEXT NOT NULL CHECK (vote_type IN ('too_strong', 'too_weak')),
    user_fingerprint TEXT NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UNIQUE(card_id, user_fingerprint)
);

-- Create index for faster queries
CREATE INDEX IF NOT EXISTS idx_card_votes_card_id ON card_votes(card_id);
CREATE INDEX IF NOT EXISTS idx_card_votes_fingerprint ON card_votes(user_fingerprint);

-- Enable Row Level Security
ALTER TABLE card_votes ENABLE ROW LEVEL SECURITY;

-- Policy: Anyone can read votes
CREATE POLICY "Anyone can view votes"
    ON card_votes
    FOR SELECT
    USING (true);

-- Policy: Anyone can insert votes (one per user per card, enforced by UNIQUE constraint)
CREATE POLICY "Anyone can vote"
    ON card_votes
    FOR INSERT
    WITH CHECK (true);

-- Policy: Users can only delete their own votes (for canceling)
CREATE POLICY "Users can delete own votes"
    ON card_votes
    FOR DELETE
    USING (true);

-- Create a view for vote statistics
CREATE OR REPLACE VIEW card_vote_stats AS
SELECT
    card_id,
    COUNT(CASE WHEN vote_type = 'too_strong' THEN 1 END) as too_strong_count,
    COUNT(CASE WHEN vote_type = 'too_weak' THEN 1 END) as too_weak_count,
    COUNT(*) as total_votes
FROM card_votes
GROUP BY card_id;

-- Grant access to the view
GRANT SELECT ON card_vote_stats TO anon, authenticated;
